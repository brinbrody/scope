using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text.Json.Serialization;
using System.Timers;
using System.Xml.Serialization;
using static DGScope.Library.Constants;

namespace DGScope.Library
{
    [Serializable()]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ColorSet
    {
        private int mapa;
        private int mapb;
        public ScopeColor DataBlockUnowned { get; set; }
        public ScopeColor DataBlockUnownedBlink { get; set; }
        public ScopeColor ActiveAlert { get; set; }
        public ScopeColor AcknowledgedAlert { get; set; }
        public ScopeColor HandoffAttention { get; set; }
        public ScopeColor PointoutAttention { get; set; }
        public ScopeColor PointoutAccepted { get; set; }
        public ScopeColor PartialDataBlockUnowned { get; set; }
        public ScopeColor PartialDataBlockUnownedBlink { get; set; }
        public ScopeColor PartialActiveAlert { get; set; }
        public ScopeColor PartialAcknowledgedAlert { get; set; }
        public ScopeColor GhostDataBlock { get; set; }
        public ScopeColor PrimaryTarget { get; set; }
        public ScopeColor BeaconTarget { get; set; }
        public ScopeColor[] History { get; set; }
        public ScopeColor PTLMinSep { get; set; }
        public ScopeColor PosOwned { get; set; }
        public ScopeColor UnownedPosOutline { get; set; }
        public ScopeColor PosOutline { get; set; }
        public ScopeColor PartialPosOutline { get; set; }
        public ScopeColor Caution { get; set; }
        public ScopeColor Owned { get; set; }
        public ScopeColor OwnedBlink { get; set; }
        public ScopeColor Highlight { get; set; }
        public ScopeColor HighlightBlink { get; set; }
        public ScopeColor PosHighlight { get; set; }
        public ScopeColor PosHighlightBlink { get; set; }
        public ScopeColor PosOwnedBlink { get; set; }
        public ScopeColor PosHandoffAttention { get; set; }
        public ScopeColor PosPointoutAttention { get; set; }
        public ScopeColor PosPointoutAccepted { get; set; }
        public ScopeColor TPA { get; set; }
        public ScopeColor RBL { get; set; }
        public int MapASelected { get; set; }
        public int MapBSelected { get; set; }
        public ScopeColor[] MapA { get; set; }
        public ScopeColor[] MapB { get; set; }
        public ScopeColor MapAColor
        {
            get
            {
                if (MapA.Length == 0)
                    return new ScopeColor(0, 0, 0);
                if (MapASelected >= MapA.Length)
                    return MapA.Last();
                return MapA[MapASelected];
            }
        }
        public ScopeColor MapBColor
        {
            get
            {
                if (MapB.Length == 0)
                    return new ScopeColor(0, 0, 0);
                if (MapBSelected >= MapB.Length)
                    return MapA.Last();
                return MapB[MapBSelected];
            }
        }
        public ScopeColor CompassRose { get; set; }
        public ScopeColor RangeRings { get; set; }
        public ScopeColor WXPattern { get; set; }
        public ScopeColor NormalListText { get; set; }
        public ScopeColor ListTitle { get; set; }
        public ScopeColor AlertListText { get; set; }
        public ScopeColor SystemStatusWxText { get; set; }
        public ScopeColor Background { get; set; }
        public ScopeColor PreviewList { get; set; }
        public ScopeColor ListBlink { get; set; }
        public WXColor[] WXColors { get; set; }
    }

    [Serializable()]
    public class ScopeColor
    {
        
        private static Timer blinkTimer;
        private static bool blinkActive = false;
        public ScopeColor(Color BaseColor, bool Blinking = false)
        {
            this.BaseColor = BaseColor;
            this.Blinking = Blinking;
        }
        public ScopeColor()
        {
              
        }
        public ScopeColor(ScopeColor color)
        {
            BaseColor = color.BaseColor;
            Blinking = color.Blinking;
        }
        public ScopeColor(int r, int g, int b, bool blinking = false)
        {
            BaseColor = Color.FromArgb(r, g, b);
            Blinking = blinking;
        }
        public ScopeColor(int a, int r, int g, int b, bool blinking = false)
        {
            BaseColor = Color.FromArgb(a, r, g, b);
            Blinking = blinking;
        }
        public ScopeColor(int argb, bool blinking = false)
        {
            BaseColor = Color.FromArgb(argb);
            Blinking = blinking;
        }

        [XmlIgnore]
        [JsonIgnore]
        public Color BaseColor { get; set; }

        [XmlElement("BaseColor")]
        [JsonPropertyName("BaseColor")]
        [Browsable(false)]
        public int BaseColorAsArgb
        {
            get { return BaseColor.ToArgb(); }
            set { BaseColor = Color.FromArgb(value); }
        }
        public bool Blinking { get; set; } = false;
        public Color GetColor(double brightness = 100)
        {
            if (brightness == 0)
                return Color.Transparent;
            brightness /= 100;
            Color color = Color.FromArgb((int)(BaseColor.R * brightness), (int)(BaseColor.G * brightness), (int)(BaseColor.B * brightness));
            if (!Blinking || blinkActive)
                return color;
            else if (blinkTimer == null)
            {
                blinkTimer = new Timer(BLINK_INTERVAL);
                blinkTimer.Start();
                blinkTimer.Elapsed += FlashTimer_Elapsed;
            }
            return Color.FromArgb((int)(color.R * BLINK_INTENSITY), (int)(color.G * BLINK_INTENSITY), (int)(color.B * BLINK_INTENSITY));
        }

        private void FlashTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            blinkActive = !blinkActive;
        }
        public override string ToString()
        {
            if (Blinking)
                return "Blinking " + BaseColor.Name;
            return BaseColor.Name;
        }
        public static implicit operator Color(ScopeColor color)
        {
            return color.GetColor();
        }
        public static implicit operator ScopeColor(Color color)
        {
            return new ScopeColor(color);
        }

        public static ScopeColor Green { get => new ScopeColor(0, 255, 0); }
        public static ScopeColor BlinkingGreen { get => new ScopeColor(0, 255, 0, true); }
        public static ScopeColor Red { get => new ScopeColor(255, 0, 0); }
        public static ScopeColor BlinkingRed { get => new ScopeColor(255, 0, 0, true); }
        public static ScopeColor White { get => new ScopeColor(255, 255, 255); }
        public static ScopeColor BlinkingWhite { get => new ScopeColor(255, 255, 255, true); }
        public static ScopeColor Cyan { get => new ScopeColor(0, 255, 255); }
        public static ScopeColor BlinkingCyan { get => new ScopeColor(0, 255, 255, true); }
        public static ScopeColor Yellow { get => new ScopeColor(255, 255, 0); }
        public static ScopeColor BlinkingYellow { get => new ScopeColor(255, 255, 0, true); }
        public static ScopeColor SearchBlue { get => new ScopeColor(30, 120, 255); }
        public static ScopeColor Blue1 { get => new ScopeColor(30, 80, 200); }
        public static ScopeColor Blue2 { get => new ScopeColor(70, 70, 170); }
        public static ScopeColor Blue3 { get => new ScopeColor(50, 50, 130); }
        public static ScopeColor Blue4 { get => new ScopeColor(40, 40, 110); }
        public static ScopeColor Blue5 { get => new ScopeColor(30, 30, 90); }
        public static ScopeColor Black { get => new ScopeColor(0, 0, 0); }
        public static ScopeColor TPABlue { get => new ScopeColor(90, 80, 255); }
        public static ScopeColor Orange { get => new ScopeColor(255, 55, 0); }
        public static ScopeColor DimGray { get => new ScopeColor(140, 140, 140); }
        public static ScopeColor Gray { get => new ScopeColor(128, 128, 128); }
        public static ScopeColor LightBlue { get => new ScopeColor(173, 216, 230); }
        public static ScopeColor Magenta { get => new ScopeColor(255, 0, 255); }
        public static ScopeColor Gold1 { get => new ScopeColor(238, 201, 0); }
        public static ScopeColor Coral2 { get => new ScopeColor(238, 106, 80); }
        public static ScopeColor DarkOliveGreen { get => new ScopeColor(162, 205, 90); }
        public static ScopeColor GoldenRod { get => new ScopeColor(Color.Goldenrod); }
        public static ScopeColor RoyalBlue { get => new ScopeColor(72, 118, 255); }
        public static ScopeColor LightSlateBlue { get => new ScopeColor(132, 112, 255); }
        public static ScopeColor Aquamarine2 { get => new ScopeColor(118, 238, 198); }
        public static ScopeColor Carrot { get => new ScopeColor(237, 145, 33); }
        public static ScopeColor Orchid { get => new ScopeColor(218, 112, 214); }
        public static ScopeColor RosyBrown2 { get => new ScopeColor(238, 180, 180); }
        public static ScopeColor LimeGreen { get => new ScopeColor(50, 205, 50); }
        public static ScopeColor IndianRed { get => new ScopeColor(255, 106, 106); }
        public static ScopeColor DarkGrayBlue { get => new ScopeColor(38, 77, 77); }
        public static ScopeColor DarkMustard { get => new ScopeColor(100, 100, 51); }
    }
    
}
