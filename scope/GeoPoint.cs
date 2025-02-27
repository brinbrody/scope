﻿using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;

namespace DGScope
{
    [Serializable()]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [JsonObject]
    public class GeoPoint
    {
        [DisplayName("Latitude")]
        [JsonProperty("Latitude")]
        public double Latitude { get; set; }
        [DisplayName("Longitude")]
        [JsonProperty("Longitude")]
        public double Longitude { get; set; }
        public double BearingTo(GeoPoint From)
        {
            double λ1 = Longitude * (Math.PI / 180);
            double λ2 = From.Longitude * (Math.PI / 180);
            double φ1 = Latitude * (Math.PI / 180);
            double φ2 = From.Latitude * (Math.PI / 180);

            double y = Math.Sin(λ2 - λ1) * Math.Cos(φ2);
            double x = Math.Cos(φ1) * Math.Sin(φ2) -
                      Math.Sin(φ1) * Math.Cos(φ2) * Math.Cos(λ2 - λ1);
            double θ = Math.Atan2(y, x);
            return (θ * (180 / Math.PI)) % 360; // in degrees

        }
        public GeoPoint(double Latitude, double Longitude)
        {
            this.Latitude = Latitude;
            this.Longitude = Longitude;
        }

        public override string ToString()
        {
            return Latitude.ToString() + ", " + Longitude.ToString();
        }
        public GeoPoint() { }

        public double DistanceTo(GeoPoint From, double Altitude = 0)
        {
            double R = 3443.92; // nautical miles
            double φ2 = Latitude * (Math.PI / 180);
            double φ1 = From.Latitude * (Math.PI / 180);
            double Δφ = (From.Latitude - Latitude) * Math.PI / 180;
            double Δλ = (From.Longitude - Longitude) * Math.PI / 180;

            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                      Math.Cos(φ1) * Math.Cos(φ2) *
                      Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double alt = Altitude / 6076.12;

            double dist = Math.Sqrt((R * c) * (R * c) + (alt * alt));
            return dist;
        }
        public GeoPoint FromPoint(double Distance, double Bearing)
        {
            return FromPoint(this, Distance, Bearing);
        }

        public static GeoPoint FromPoint(GeoPoint Origin, double Distance, double Bearing)
        {
            double R = 3443.92; // nautical miles
            double brng = Bearing * (Math.PI / 180);
            double d = Distance;
            double φ1 = Origin.Latitude * (Math.PI / 180);
            double λ1 = Origin.Longitude * (Math.PI / 180);
            double φ2 = Math.Asin(Math.Sin(φ1) * Math.Cos(d / R) +
                      Math.Cos(φ1) * Math.Sin(d / R) * Math.Cos(brng));
            double λ2 = λ1 + Math.Atan2(Math.Sin(brng) * Math.Sin(d / R) * Math.Cos(φ1),
                           Math.Cos(d / R) - Math.Sin(φ1) * Math.Sin(φ2));
            double newLatitude = φ2 * (180 / Math.PI);
            double newLongitude = λ2 * (180 / Math.PI);
            return new GeoPoint(newLatitude, newLongitude);

        }

        public static bool TryParse(string pointString, out GeoPoint point)
        {
            point = new GeoPoint();
            pointString = pointString.Trim();
            var pointSplit = pointString.Split(' ');
            if (pointSplit.Length < 2)
                return false;
            var lat = pointSplit[0].Split('.');
            var lon = pointSplit[1].Split('.');
            double value = 0;
            double latitude = 0;
            if (lat.Length == 2 && double.TryParse(pointSplit[0], out latitude))
            {

            }
            else //  Try VRC format
            {
                if (lat[0].Length < 2)
                    return false;
                if (double.TryParse(lat[0].Substring(1), out value))
                    latitude += value;
                else
                    return false;
                if (lat.Length > 2 && double.TryParse(lat[1], out value))
                    latitude += value / 60;
                if (lat.Length >= 3 && double.TryParse(lat[2], out value))
                    latitude += value / 3600;
                if (lat.Length >= 4 && double.TryParse(lat[3], out value))
                    latitude += value / 3600000;
            }
            if (pointSplit[0].Contains('S'))
                latitude *= -1;

            double longitude = 0;
            if (lon.Length == 2 && double.TryParse(pointSplit[1], out longitude))
            {

            }
            else //  Try VRC format
            {
                if (double.TryParse(lon[0].Substring(1), out value))
                    longitude += value;
                else
                    return false;
                if (lon.Length > 2 && double.TryParse(lon[1], out value))
                    longitude += value / 60;
                if (lon.Length >= 3 && double.TryParse(lon[2], out value))
                    longitude += value / 3600;
                if (lon.Length >= 4 && double.TryParse(lon[3], out value))
                    longitude += value / 3600000;
            }
            if (pointSplit[1].Contains('W'))
                longitude *= -1;
            point.Latitude = latitude;
            point.Longitude = longitude;
            return true;
        }
    }
}
