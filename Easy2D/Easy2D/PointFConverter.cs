using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Easy2D
{
    public class PointFConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            // Erlaubt die Konvertierung von einem string zu PointF
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string s = (string)value;
                Regex regex = new Regex(@"{(?<x>[-+]?\d+(\.\d+)?):(?<y>[-+]?\d+(\.\d+)?)\}");
                Match match = regex.Match(s);
                if (match.Success)
                {
                    float x = float.Parse(match.Groups["x"].Value, CultureInfo.InvariantCulture);
                    float y = float.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture);
                    return new PointF(x, y);
                }
            }
            return PointF.Empty;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                PointF point = (PointF)value;
                return "{" + point.X.ToString(CultureInfo.InvariantCulture) + ":" + point.Y.ToString(CultureInfo.InvariantCulture) + "}";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

}
