using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuntasticViewer.Json;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace RuntasticViewer
{
    public partial class MainPage : ContentPage
    {
        private Extent _extent = new Extent(0, 0, 0, 0);
        private Position _center = new Position();

        public MainPage()
        {
            InitializeComponent();

            var assembly = typeof(MainPage).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("RuntasticViewer.Assets.Resources.defaultTrace.json");

            if (stream == null) return;

            using (var sr = new StreamReader(stream))
            {
                var content = sr.ReadToEnd();
                var trace = JsonConvert.DeserializeObject<RuntasticTrace>(content);
                var poly = TraceToPolyline(trace);
                Map.Polylines.Add(poly);

                _extent = FindPolylineExtent(poly);
                _center = new Position(
                    (_extent.minLat + _extent.maxLat) / 2,
                    (_extent.minLng + _extent.maxLng) / 2
                );

                poly.Clicked += (sender, args) => { Console.WriteLine("Clicked poly"); };

                // we need to move to the map asynchronously since it doesn't work if we do it right when the map is loaded
                var timer = new System.Timers.Timer(.1);
                timer.Elapsed += (sender, args) =>
                    Map.MoveToRegion(new MapSpan(_center, _extent.width, _extent.height));
                timer.Start();
            }

            Fly.Clicked += (sender, args) => { Map.MoveToRegion(new MapSpan(_center, _extent.width, _extent.height)); };
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

            poly.IsClickable = true;
            poly.StrokeColor = Color.Blue;
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