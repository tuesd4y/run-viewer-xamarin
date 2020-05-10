using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microcharts.Forms;
using Microcharts;
using Newtonsoft.Json;
using RuntasticViewer.Json;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Microcharts.Forms;
using SkiaSharp;
using Xamarin.Forms.GoogleMaps.Extensions;
using Xamarin.Forms.Internals;
using Entry = Microcharts.Entry;

namespace RuntasticViewer
{
    public partial class MainPage : ContentPage
    {
        private Extent _extent = new Extent(0, 0, 0, 0);
        private Position _center = new Position();
        private RuntasticTrace _trace = null;
        private Polyline _poly = new Polyline();

        // todo use a place-queue instead of that
        private int _start = -1;
        private int _end = -1;

        private Queue<Pin> _pins = new Queue<Pin>();

        public MainPage()
        {
            BindingContext = this;

            InitializeComponent();

            var assembly = typeof(MainPage).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("RuntasticViewer.Assets.Resources.defaultTrace.json");

            if (stream == null) return;

            using (var sr = new StreamReader(stream))
            {
                var content = sr.ReadToEnd();
                _trace = JsonConvert.DeserializeObject<RuntasticTrace>(content);
                _poly = TraceToPolyline(_trace);
                Map.Polylines.Add(_poly);

                _extent = FindPolylineExtent(_poly);
                _center = new Position(
                    (_extent.minLat + _extent.maxLat) / 2,
                    (_extent.minLng + _extent.maxLng) / 2
                );


                Map.InitialCameraUpdate = CameraUpdateFactory
                    .NewBounds(new MapSpan(_center, _extent.width, _extent.height).ToBounds(),
                        10);
                // we only need to do this once, so we can stop the timer after we executed it one time
            }


            Map.MapClicked += MapClicked;

            Fly.Clicked += (sender, args) => { Map.MoveToRegion(new MapSpan(_center, _extent.width, _extent.height)); };

            CalculateHeightChart(0, _trace.Features[0].Geometry.Coordinates.Length - 1);
        }

        /// <summary>
        /// Fills the height chart with 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void CalculateHeightChart(int start, int end)
        {
            var selectedHeights = _trace.Features[0].Geometry.Coordinates
                .Where((_, id) => id >= start && id <= end)
                .Select(coords => coords[2])
                .ToList();

            var points = _trace.Features[0].Geometry.Coordinates
                .Where((_, id) => id >= start && id <= end)
                .Select(coords => new Position(coords[1], coords[0]))
                .ToList();

            // list for all average relative heights
            var heightEntries = new List<Entry>();
            var startHeight = selectedHeights.First();

            var speedEntries = new List<Entry>();

            var max = end > 50 ? selectedHeights.Count / 25.0 : 1.0;
            for (var i = 0; i + max < selectedHeights.Count; i = (int) (i + max))
            {
                var heightSum = 0.0;
                for (var j = 0; j < max; j++)
                {
                    heightSum += selectedHeights[i + j];
                }

                var distance = 0.0;
                var p = points.Where((_, id) => id >= i && id <= i + max).ToList();
                var last = p.First();
                foreach (var v in p.Skip(1))
                {
                    distance += last.Distance(v);
                    last = v;
                }

                var h = (float) (startHeight - (end > 50 ? heightSum / max : heightSum));

                heightEntries.Add(new Entry(h)
                {
                    Color = h > 0 ? SKColors.LimeGreen : SKColors.OrangeRed
                });

                // calculate average speed over last legs
                var startTime = _trace.Features[0].Properties.CoordTimes[start + i];
                var endTime = _trace.Features[0].Properties.CoordTimes[(int) Math.Round(start + i + max)];
                var time = endTime - startTime;
                var speed = (float) (time.TotalMinutes / (distance / 1000));
                
                speedEntries.Add(new Entry(speed)
                {
                    // value label should only be shown on every third item to not be too crowded
                    ValueLabel = i % 3 == 0 ? $"{speed:N1} m/km " : "",
                    Color = speedEntries.Count > 0 ? (speed - speedEntries.First().Value > 0 ? SKColors.OrangeRed : SKColors.LimeGreen) : SKColors.LimeGreen
                });
            }

            var entries = heightEntries.ToArray();

            HeightChart.Chart = new LineChart()
            {
                Entries = entries.ToArray(),
                PointMode = PointMode.None
            };

            SpeedChart.Chart = new LineChart()
            {
                Entries = speedEntries.ToArray(),
                PointMode = PointMode.Square
            };
        }

        private void MapClicked(object sender, MapClickedEventArgs args)
        {
            var pos = args.Point;

            // find the nearest point in the polyline vertices
            // this is right now not exactly the nearest point, but the nearest turning point in the polyline.
            // to do this better, we could check out the implementation in this StackOverflow answer
            // https://stackoverflow.com/questions/16429562/find-a-point-in-a-polyline-which-is-closest-to-a-latlng
            var nearestPosTuple = _poly.Positions
                .Select(((position, i) => new Tuple<double, int>(position.Distance(pos), i)))
                .OrderBy((tuple => tuple.Item1))
                .First();

            var nearestPos = _poly.Positions[nearestPosTuple.Item2];
            var distance = nearestPosTuple.Item1;

            // only take the point if distance is less than a 100 meters away
            if (distance >= 100)
                return;

            if (_start == -1 || _end != -1)
            {
                _start = nearestPosTuple.Item2;
                _end = -1;
            }
            else
            {
                _end = nearestPosTuple.Item2;
            }


            var p = new Pin
            {
                Position = nearestPos,
                Type = PinType.Place,
                Label = "Start",
                Address = $"id: {nearestPosTuple.Item2}"
            };
            
            // check that only two pins are present at a given time: 
            _pins.Enqueue(p);
            if (_pins.Count > 2)
            {
                var toDelete = _pins.Dequeue();
                Map.Pins.Remove(toDelete);
            }
            Map.Pins.Add(p);
        }

        /// <summary>
        /// Convert the linestring that is contained in this trace to a polyline geometry. 
        /// </summary>
        /// <param name="trace">the trace to convert</param>
        /// <returns>a polyline that represents the geometry in this trace</returns>
        private static Polyline TraceToPolyline(RuntasticTrace trace)
        {
            var poly = new Polyline();
            // there should only be one feature (a LineString) in there, otherwise it's not a correct runtastic trace.
            var linestring = trace.Features.First();
            foreach (var coords in linestring.Geometry.Coordinates)
            {
                poly.Positions.Add(new Position(coords[1], coords[0]));
            }

            poly.IsClickable = false;
            poly.StrokeColor = Color.YellowGreen;
            poly.StrokeWidth = 5f;

            return poly;
        }


        /// <summary>
        /// Return the geographic extent of the geometry described in the polyline
        /// </summary>
        /// <param name="poly">The polyline to create an extent from</param>
        /// <returns>the extent of the polyline in the form of minLat, maxLat, minLng, maxLng</returns>
        private static Extent FindPolylineExtent(Polyline poly)
        {
            var extent = new Extent(double.PositiveInfinity, double.NegativeInfinity, double.PositiveInfinity,
                double.NegativeInfinity);

            foreach (var pos in poly.Positions)
            {
                if (pos.Latitude < extent.minLat) extent.minLat = pos.Latitude;
                if (pos.Latitude > extent.maxLat) extent.maxLat = pos.Latitude;
                if (pos.Longitude < extent.minLng) extent.minLng = pos.Longitude;
                if (pos.Longitude > extent.maxLng) extent.maxLng = pos.Longitude;
            }

            return extent;
        }

        private class Extent
        {
            public double minLat { get; set; }
            public double maxLat { get; set; }
            public double minLng { get; set; }
            public double maxLng { get; set; }


            public double width => maxLat - minLat;

            public double height => maxLng - minLng;

            public Extent(double minLat, double maxLat, double minLng, double maxLng)
            {
                this.minLat = minLat;
                this.maxLat = maxLat;
                this.minLng = minLng;
                this.maxLng = maxLng;
            }
        }
    }
}