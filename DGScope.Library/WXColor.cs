using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace DGScope.Library
{
    [Serializable()]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class WXColor : ScopeColor
    {
        public byte[] StipplePattern { get; set; }
        public WXColor(ScopeColor baseColor, byte[] pattern = null) : base(baseColor)
        {
            StipplePattern = pattern;
        }
        public WXColor() : base() { }
    }
}
