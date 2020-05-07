using System;
using System.Runtime;
using Xamarin.Forms.GoogleMaps;

namespace RuntasticViewer
{
    public static class Extensions
    {
        public static double SimpleDistance(this Position position, Position other)
        {
            return Math.Sqrt(Math.Pow(position.Latitude - other.Latitude, 2) +
                             Math.Pow(position.Longitude - other.Longitude, 2));
        }
        public static double Distance(this Position position, Position other) => Haversine.Distance(position, other);
    }

    public static class Haversine
    {
        /// <summary>
        /// Calculate haversine distance between two points (assuming coordinates in WGS84)
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        public static double Distance(Position pos1, Position pos2)
        {
            var r = 6_371_000.0f;

            var dLat = ToRadian(pos2.Latitude - pos1.Latitude);
            var dLon = ToRadian(pos2.Longitude - pos1.Longitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadian(pos1.Latitude)) * Math.Cos(ToRadian(pos2.Latitude)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            var d = r * c;

            return d;
        }

        private static double ToRadian(double val)
        {
            return (Math.PI / 180) * val;
        }
    }
}