using System;

namespace DGScope.Library
{
    public class AltitudeFilter
    {
        public int AssociatedMin { get; set; } = -9900;
        public int AssociatedMax { get; set; } = 99900;
        public int UnassociatedMin { get; set; } = -9900;
        public int UnassociatedMax { get; set; } = 99900;
        public override string ToString()
        {
            string output = "";
            if (AssociatedMin < 0)
                output = "N";
            output += Math.Abs(AssociatedMin / 100) + " ";
            if (AssociatedMax < 0)
                output += "N";
            output += Math.Abs(AssociatedMax / 100) + "\r\n";
            if (UnassociatedMin < 0)
                output = "N";
            output += Math.Abs(AssociatedMin / 100) + " ";
            if (UnassociatedMax < 0)
                output += "N";
            output += Math.Abs(UnassociatedMax / 100);
            return output;
        }
    }
}
