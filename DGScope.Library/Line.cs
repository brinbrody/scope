using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DGScope.Library
{
    public class Line
    {
        public GeoPoint End1 { get; set; } = new GeoPoint();
        public GeoPoint End2 { get; set; } = new GeoPoint();
        [JsonIgnore]
        [Browsable(false)]
        public double Direction { get => End1.BearingTo(End2); }
        [JsonIgnore]
        [Browsable(false)]
        public double Length { get => End1.DistanceTo(End2); }
        public Line(GeoPoint End1, GeoPoint End2) { this.End1 = End1; this.End2 = End2; }
        public Line() { }
        
    }
}
