using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

using Easy2D.Components;

namespace Easy2D
{

	public class PortListConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
            List<Components.Component> ports = (List<Components.Component>)value;
			return string.Join(", ", Enumerable.Select(ports, (global::Easy2D.Components.Component port) => port.Name ?? ""));
		}
	}
}
