using System.Collections.Generic;
using System.Drawing;

namespace DGScope.Library
{
    public class PrefSet
    {
        public double DisplayRange { get; set; }
        public GeoPoint DisplayCenter { get; set; } = new GeoPoint();
        public PointF SystemStatusAreaLocation { get; set; }
        public PointF PreviewAreaLocation { get; set; }
        //aircraft list locations & visibility
        public DCBLocation DCBLocation { get; set; } = DCBLocation.Top;
        public bool DisplayCompass { get; set; } = false;
        public bool DisplayRangeRings { get; set; } = true;
        public int RangeRingSpacing { get; set; } = 5;
        public GeoPoint? RangeRingCenter { get; set; }
        public List<int> DisplayedMaps { get; set; } = new List<int>();
        public List<string> QuickLook { get; set; } = new List<string>();
        public AltitudeFilter AltitudeFilter { get; set; } = new AltitudeFilter();
        //enable / inhibit settings for altitude filter override volumes
        public BrightnessSettings BrightnessSettings { get; set; } = new BrightnessSettings();
        public int HistoryNum { get; set; } = 10;
        public double HistoryRate { get; set; } = 4.5;
        public FontSizes FontSizes { get; set; } = new FontSizes();
        public PTLSettings PTLSettings { get; set; } = new PTLSettings();
        public LDRDirection LDRDirection { get; set; } = LDRDirection.N;
        public int LDRLength { get; set; } = 1;
        public bool AutoPreviewClear { get; set; }
        //beacon block selections
        // readout area auto clear enable


        // selected map list category
        public bool DwellEnable { get; set; } = false;

        //TPA display settings

        public char RadarSite { get; set; } = '0';
        public PrefSet() { }
        public PrefSet(Adaptation adaptation)
        {
            DisplayRange = adaptation.MaxRange;
            DisplayCenter = adaptation.FacilityCenter;
            RangeRingCenter = adaptation.FacilityCenter;

        }
    }
}
