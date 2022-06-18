using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using static DGScope.Library.Constants;

namespace DGScope.Library
{
    public class Adaptation
    {
        public string Name { get; set; }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public GeoPoint FacilityCenter { get; set; } = new GeoPoint();
        public int MaxRange { get; set; } = 100;
        public double MagVar { get; set; } = 0;
        public ColorSet TCWColors { get; set; } = new ColorSet()
        {
            DataBlockUnowned = ScopeColor.Green,
            DataBlockUnownedBlink = ScopeColor.BlinkingGreen,
            ActiveAlert = ScopeColor.BlinkingRed,
            AcknowledgedAlert = ScopeColor.Red,
            HandoffAttention = ScopeColor.BlinkingWhite,
            PointoutAttention = ScopeColor.BlinkingYellow,
            PointoutAccepted = ScopeColor.Yellow,
            PartialDataBlockUnowned = ScopeColor.Green,
            PartialDataBlockUnownedBlink = ScopeColor.BlinkingGreen,
            GhostDataBlock = ScopeColor.Yellow,
            PrimaryTarget = ScopeColor.SearchBlue,
            BeaconTarget = ScopeColor.Green,
            History = new ScopeColor[]
            {
                ScopeColor.Blue1,
                ScopeColor.Blue2,
                ScopeColor.Blue3,
                ScopeColor.Blue4,
                ScopeColor.Blue5
            },
            PTLMinSep = ScopeColor.White,
            PosOwned = ScopeColor.White,
            PosOutline = ScopeColor.Black,
            PartialPosOutline = ScopeColor.Black,
            Caution = ScopeColor.BlinkingYellow,
            Owned = ScopeColor.White,
            OwnedBlink = ScopeColor.BlinkingWhite,
            Highlight = ScopeColor.Cyan,
            HighlightBlink = ScopeColor.BlinkingCyan,
            PosHighlight = ScopeColor.Cyan,
            PosHighlightBlink = ScopeColor.BlinkingCyan,
            PartialAcknowledgedAlert = ScopeColor.Red,
            PartialActiveAlert = ScopeColor.BlinkingRed,
            PosHandoffAttention = ScopeColor.BlinkingWhite,
            PosOwnedBlink = ScopeColor.BlinkingWhite,
            PosPointoutAccepted = ScopeColor.Yellow,
            PosPointoutAttention = ScopeColor.BlinkingYellow,
            UnownedPosOutline = ScopeColor.Black,
            RBL = ScopeColor.White,
            MapASelected = 0,
            MapBSelected = 0,
            MapA = new ScopeColor[]
            {
                ScopeColor.DimGray,
                ScopeColor.Cyan,
                ScopeColor.Magenta,
                ScopeColor.Gold1,
                ScopeColor.Coral2,
                ScopeColor.DarkOliveGreen,
                ScopeColor.GoldenRod,
                ScopeColor.RoyalBlue
            },
            MapB = new ScopeColor[]
            {
                ScopeColor.DimGray,
                ScopeColor.LightSlateBlue,
                ScopeColor.Aquamarine2,
                ScopeColor.Carrot,
                ScopeColor.Orchid,
                ScopeColor.RosyBrown2,
                ScopeColor.LimeGreen,
                ScopeColor.IndianRed
            },
            NormalListText = ScopeColor.Green,
            ListTitle = ScopeColor.Green,
            AlertListText = ScopeColor.Red,
            Background = ScopeColor.Black,
            PreviewList = ScopeColor.Green,
            TPA = ScopeColor.TPABlue,
            ListBlink = ScopeColor.BlinkingGreen,
            CompassRose = ScopeColor.DimGray,
            RangeRings = ScopeColor.DimGray,
            SystemStatusWxText = ScopeColor.Cyan,
            WXColors = new WXColor[]
            {
                new WXColor(ScopeColor.DarkGrayBlue),
                new WXColor(ScopeColor.DarkGrayBlue, LIGHT_STIPPLE_PATTERN),
                new WXColor(ScopeColor.DarkGrayBlue, DENSE_STIPPLE_PATTERN),
                new WXColor(ScopeColor.DarkMustard),
                new WXColor(ScopeColor.DarkMustard, LIGHT_STIPPLE_PATTERN),
                new WXColor(ScopeColor.DarkMustard, DENSE_STIPPLE_PATTERN)
            }
        };
        public ColorSet TDWColors { get; set; } = new ColorSet()
        {
            DataBlockUnowned = ScopeColor.White,
            DataBlockUnownedBlink = ScopeColor.BlinkingWhite,
            ActiveAlert = ScopeColor.BlinkingRed,
            AcknowledgedAlert = ScopeColor.Red,
            HandoffAttention = ScopeColor.BlinkingWhite,
            PointoutAttention = ScopeColor.BlinkingYellow,
            PointoutAccepted = ScopeColor.Yellow,
            PartialDataBlockUnowned = ScopeColor.White,
            PartialDataBlockUnownedBlink = ScopeColor.BlinkingWhite,
            GhostDataBlock = ScopeColor.Yellow,
            PrimaryTarget = ScopeColor.SearchBlue,
            BeaconTarget = ScopeColor.Green,
            History = new ScopeColor[]
            {
                ScopeColor.Blue1,
                ScopeColor.Blue2,
                ScopeColor.Blue3,
                ScopeColor.Blue4,
                ScopeColor.Blue5
            },
            PTLMinSep = ScopeColor.White,
            PosOwned = ScopeColor.White,
            PosOutline = ScopeColor.Black,
            PartialPosOutline = ScopeColor.Black,
            Caution = ScopeColor.BlinkingYellow,
            Owned = ScopeColor.White,
            OwnedBlink = ScopeColor.BlinkingWhite,
            Highlight = ScopeColor.Cyan,
            HighlightBlink = ScopeColor.BlinkingCyan,
            PosHighlight = ScopeColor.Cyan,
            PosHighlightBlink = ScopeColor.BlinkingCyan,
            PartialAcknowledgedAlert = ScopeColor.Red,
            PartialActiveAlert = ScopeColor.BlinkingRed,
            PosHandoffAttention = ScopeColor.BlinkingWhite,
            PosOwnedBlink = ScopeColor.BlinkingWhite,
            PosPointoutAccepted = ScopeColor.Yellow,
            PosPointoutAttention = ScopeColor.BlinkingYellow,
            UnownedPosOutline = ScopeColor.Black,
            RBL = ScopeColor.White,
            MapASelected = 0,
            MapBSelected = 0,
            MapA = new ScopeColor[]
            {
                ScopeColor.DimGray,
                ScopeColor.Cyan,
                ScopeColor.Magenta,
                ScopeColor.Gold1,
                ScopeColor.Coral2,
                ScopeColor.DarkOliveGreen,
                ScopeColor.GoldenRod,
                ScopeColor.RoyalBlue
            },
            MapB = new ScopeColor[]
            {
                ScopeColor.DimGray,
                ScopeColor.LightSlateBlue,
                ScopeColor.Aquamarine2,
                ScopeColor.Carrot,
                ScopeColor.Orchid,
                ScopeColor.RosyBrown2,
                ScopeColor.LimeGreen,
                ScopeColor.IndianRed
            },
            WXColors = new WXColor[]
            {
                new WXColor(ScopeColor.DarkGrayBlue),
                new WXColor(ScopeColor.DarkGrayBlue, LIGHT_STIPPLE_PATTERN),
                new WXColor(ScopeColor.DarkGrayBlue, DENSE_STIPPLE_PATTERN),
                new WXColor(ScopeColor.DarkMustard),
                new WXColor(ScopeColor.DarkMustard, LIGHT_STIPPLE_PATTERN),
                new WXColor(ScopeColor.DarkMustard, DENSE_STIPPLE_PATTERN)
            },
            WXPattern = ScopeColor.White,
            NormalListText = ScopeColor.Green,
            ListTitle = ScopeColor.Green,
            AlertListText = ScopeColor.Red,
            Background = ScopeColor.Black,
            PreviewList = ScopeColor.Green,
            TPA = ScopeColor.TPABlue,
            ListBlink = ScopeColor.BlinkingGreen,
            CompassRose = ScopeColor.DimGray,
            RangeRings = ScopeColor.DimGray,
            SystemStatusWxText = ScopeColor.Cyan

        };
        public double DataBlockTimeshareInterval { get; set; } = 1;
        public List<RadarSite> RadarSites { get; set; } = new List<RadarSite>();
        [TypeConverter(typeof(ExpandableObjectConverter))] 
        public WeatherProcessor WeatherProcessor { get; set; } = new WeatherProcessor();
        [XmlIgnore]
        [JsonIgnore]
        public VideoMapList VideoMaps { get; set; } = new VideoMapList();
        [Browsable(false)]
        public string VideoMapFileName { get; set; }
        public static string SerializeToJson(Adaptation adaptation)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(adaptation, options);
        }

        public static Adaptation DeserializeFromJson(string jsonString)
        {
            return (Adaptation)JsonSerializer.Deserialize(jsonString, typeof(Adaptation));
        }

        public static Adaptation DeserializeFromJsonFile(string filename)
        {
            string json = File.ReadAllText(filename);
            var result = DeserializeFromJson(json);
            result.VideoMaps = VideoMapList.DeserializeFromJsonFile(result.VideoMapFileName);
            return result;
        }

        public static void SerializeToJsonFile(Adaptation adaptation, string filename)
        {
            File.WriteAllText(filename, SerializeToJson(adaptation));
        }
    }
}
