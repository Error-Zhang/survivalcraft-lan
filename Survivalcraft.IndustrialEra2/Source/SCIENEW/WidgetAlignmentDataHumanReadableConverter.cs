using System;
using Engine.Serialization;

namespace SCIENEW;

[HumanReadableConverter(new Type[] { typeof(WidgetAlignmentData) })]
public class WidgetAlignmentDataHumanReadableConverter : IHumanReadableConverter
{
	public string ConvertToString(object value)
	{
		return ((WidgetAlignmentData)value).ToInt().ToString();
	}

	public object ConvertFromString(Type type, string data)
	{
		return WidgetAlignmentData.FromInt(int.Parse(data));
	}
}
