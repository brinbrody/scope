using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using DGScope.Nexrad;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace DGScope.Library
{
    public class WeatherProcessor
    {
        private double[][] vertices = new double[Constants.MAX_WX_LEVELS][];
        private object[] vertexLockObj = new object[Constants.MAX_WX_LEVELS];
        public WeatherLevel[] Levels { get; set; }
        public List<WeatherReceiver> Receivers { get; set; } = new List<WeatherReceiver>();

        public double[] GetPolygons(int level, bool max = false)
        {
            bool lockTaken = false;
            if (vertexLockObj[level] == null)
                vertexLockObj[level] = new object();
            try
            {
                Monitor.TryEnter(vertexLockObj[level], ref lockTaken);
                if (lockTaken)
                {
                    Task.Run(() => GeneratePolygons(level, max));
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(vertexLockObj[level]);
                }
            }
            if (vertices[level] == null)
                return Array.Empty<double>();
            return vertices[level];
        }

        public void GeneratePolygons(int level, bool max = false)
        {
            if (Receivers.Count == 0 || Levels == null)
            {
                vertices[level] =  Array.Empty<double>();
            }
            else if (Receivers.Count > 1)
            {
                throw new NotImplementedException("Only one weather receiver currently supported.");
            }
            var polygons = Receivers[0].GetPolygons();
            if (!max)
                max = level == Levels.Length - 1;
            var polylist = new List<WeatherPoly>();
            for (int i = 0; i < polygons.Length; i++)
            {
                if (polygons[i].dBz < Levels[level].MinDbz)
                    continue;
                else if (!max)
                    if (polygons[i].dBz >= Levels[level + 1].MinDbz)
                        continue;
                polylist.Add(polygons[i]);
            }
            double[] newvertices = new double[polylist.Count * 12];
            for (int i = 0; i < polylist.Count; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    newvertices[(12 * i) + (3 * j)] = polylist[i].Points[j].Longitude;
                    newvertices[(12 * i) + (3 * j) + 1] = polylist[i].Points[j].Latitude;
                    newvertices[(12 * i) + (3 * j) + 2] = 0;
                }
            }
            vertices[level] = newvertices;
        }
    }

    public class WeatherLevel
    {
        public double MinDbz { get; set; }
        public override string ToString()
        {
            return MinDbz.ToString();
        }
    }
    public class WeatherGridSquare
    {
        
        public RectangleF Bounds { get; set; }
        
    }

    public class WeatherReceiver
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int DownloadInterval { get; set; } = 60;
        public DateTime LastReceived { get; private set; } = DateTime.MinValue;

        private NexradDecoder decoder = new NexradDecoder();
        
        private void Download()
        {
            lock (decoder)
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        Stream response = client.OpenRead(Url);
                        decoder.Parse(response);
                    }
                    LastReceived = DateTime.Now;
                }
                catch { }
                if (decoder.SymbologyBlock.GetType() != typeof(RadialSymbologyBlock))
                    throw new NotImplementedException("Only radial symbology blocks are currently supported.");
                RadialSymbologyBlock symbologyBlock = (RadialSymbologyBlock)decoder.SymbologyBlock;
                GeoPoint radarCenter = new GeoPoint(decoder.DescriptionBlock.Latitude, decoder.DescriptionBlock.Longitude);
                double rstep = 360d / symbologyBlock.Radials.Length;
                List<WeatherPoly> newPolygons = new List<WeatherPoly>();
                for (int r = 0; r < symbologyBlock.Radials.Length; r++)
                {
                    double radial = symbologyBlock.Radials[r].StartAngle;
                    double delta = symbologyBlock.Radials[r].AngleDelta;
                    double dstep = (decoder.Range * Math.Cos(decoder.DescriptionBlock.ProductSpecific_3 * (Math.PI / 180))) / symbologyBlock.LayerNumberOfRangeBins;
                    for (int d = 0; d < symbologyBlock.Radials[r].Values.Length; d++)
                    {
                        double distance = dstep * d;
                        WeatherPoly poly = new WeatherPoly()
                        {
                            dBz = symbologyBlock.Radials[r].Values[d]
                        };
                        poly.Points = new GeoPoint[4];
                        poly.Points[0] = radarCenter.FromPoint(distance, radial);
                        poly.Points[1] = radarCenter.FromPoint(distance, radial + delta);
                        poly.Points[2] = radarCenter.FromPoint(distance + dstep, radial + delta);
                        poly.Points[3] = radarCenter.FromPoint(distance + dstep, radial);
                        newPolygons.Add(poly);
                    }
                }
                polygons = newPolygons.ToArray();
            }
        }
        WeatherPoly[] polygons = Array.Empty<WeatherPoly>();
        public WeatherPoly[] GetPolygons()
        {
            if ((DateTime.Now - LastReceived).TotalSeconds >= DownloadInterval)
                Task.Run(Download);
            return polygons;
        }
    }
}
