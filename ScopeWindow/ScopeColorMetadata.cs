using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Drawing;
using System.Timers;
using DGScope.Library;
using System;

namespace ScopeWindow
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Editor(typeof(ScopeColorEditor), typeof(UITypeEditor))]
    public class ScopeColorMetadata
    {
        [DisplayName("Base Color")]
        [Category("Color")]
        [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
        public Color BaseColor { get; set; }
        [Category("Color")]
        public bool Blinking { get; set; }
    }

    public class ScopeColorEditor : ColorEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.None;
        }
        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value is ScopeColor)
            {
                ScopeColor color = (ScopeColor)e.Value;
                SolidBrush b = new SolidBrush(color);
                e.Graphics.FillRectangle(b, e.Bounds);
                b.Dispose();
            }
        }
    }

    
    
}
