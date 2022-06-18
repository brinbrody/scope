using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public class Polygon
    {
        public ScopeColor Color { get; set; }
        public byte[] StipplePattern { get; set; }
        public ScopeColor PatternColor { get; set; }
        public GeoPoint[] Points { get; set; }
    }

    public class WeatherPoly
    {
        public GeoPoint[] Points { get; set; }
        public double dBz { get; set; } 
    }
}
