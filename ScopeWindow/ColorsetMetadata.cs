using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DGScope.Library;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace ScopeWindow
{
    public class ColorsetMetadata
    {
        [DisplayName("Map A Colors")]
        [Description("Map Category A color choices")]
        [Editor(typeof(ArrayEditor), typeof(UITypeEditor))]
        public ScopeColor[] MapA { get; set; }
        [Browsable(false)]
        public ScopeColor MapAColor { get; }

        [DisplayName("Map B Colors")]
        [Description("Map Category B color choices")]
        [Editor(typeof(ArrayEditor), typeof(UITypeEditor))]
        public ScopeColor[] MapB { get; set; }
        [TypeConverter(typeof(MapChoiceConverter))]
        [Editor(typeof(MapColorPickerUITypeEditor), typeof(UITypeEditor))]
        public ScopeColor MapBColor { get; set;  }
        [Browsable(false)]
        public int MapASelected { get; set; }

        [Editor(typeof(ArrayEditor), typeof(UITypeEditor))]
        public ScopeColor[] History { get; set; }
    }

    public class MapChoiceConverter : ColorConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<int> colors = new List<int>();
            if (context.Instance.GetType() != typeof(ColorSet))
                return null;
            var colorset = context.Instance as ColorSet;

            if (context.PropertyDescriptor.Name == "MapASelected")
            {
                return new StandardValuesCollection(colorset.MapA);
            }
            else if (context.PropertyDescriptor.Name == "MapBSelected")
            {
                return new StandardValuesCollection(colorset.MapB);
            }
            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
                return null;
            if (destinationType == typeof(string) && value.GetType() == typeof(ScopeColor))
                return (value as ScopeColor);
            if (destinationType == typeof(string) && value.GetType() == typeof(int))
                return value.ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() != typeof(string))
                return null;
            if (context.Instance.GetType() == typeof(ColorSet))
            {
                var colorset = context.Instance as ColorSet;
                ScopeColor[] colors;
                switch (context.PropertyDescriptor.Name)
                {
                    case "MapASelected":
                        colors = colorset.MapA;
                        break;
                    case "MapBSelected":
                        colors = colorset.MapA;
                        break;
                    default:
                        colors = new ScopeColor[0];
                        break;
                }
                if (colors.Length > 0)
                {
                    for (int i = 0; i < colors.Length; i++)
                    {
                        if (colors[i].ToString() == value.ToString())
                            return colors[i];
                    }
                }
            }
            return null;
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }
    }

    public class MapColorPickerUITypeEditor  : UITypeEditor
    {
        private IWindowsFormsEditorService es;
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            ColorSet colorset = context.Instance as ColorSet;
            es = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            /*
            ListBox lbx = new ListBox();
            if (es != null)
            {
                switch (context.PropertyDescriptor.Name)
                {
                    case ("MapASelected"):
                        lbx.Items.AddRange((context.Instance as ColorSet).MapA);
                        break;
                    case ("MapBSelected"):
                        lbx.Items.AddRange((context.Instance as ColorSet).MapA);
                        break;
                }
                es.DropDownControl(lbx);
            }
            int index = lbx.SelectedIndex;
            lbx.Dispose();
            */
            return base.EditValue(context, provider, value);
        }
        public override void PaintValue(PaintValueEventArgs e)
        {
            ScopeColor color = e.Value as ScopeColor;
            SolidBrush b = new SolidBrush(color);
            e.Graphics.FillRectangle(b, e.Bounds);
            b.Dispose();
        }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
