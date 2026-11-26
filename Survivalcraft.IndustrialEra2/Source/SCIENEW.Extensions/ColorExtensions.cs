using System;
using Engine;

namespace SCIENEW.Extensions;

public static class ColorExtensions
{
	public static Color BlendColors(this Color color1, Color color2, double ratio)
	{
		return (R: (byte)((double)(int)color1.R * ratio + (double)(int)color2.R * (1.0 - ratio)), G: (byte)((double)(int)color1.G * ratio + (double)(int)color2.G * (1.0 - ratio)), B: (byte)((double)(int)color1.B * ratio + (double)(int)color2.B * (1.0 - ratio)));
	}

	public static Color BlackbodyColor(this float temperature)
	{
		double num = (double)(temperature + 273.15f) / 100.0;
		double num2 = ((num <= 66.0) ? 255.0 : Math.Clamp(329.698727446 * Math.Pow(num - 60.0, -0.1332047592), 0.0, 255.0));
		double num3 = ((num <= 66.0) ? Math.Clamp(99.4708025861 * Math.Log(num) - 161.1195681661, 0.0, 255.0) : Math.Clamp(288.1221695283 * Math.Pow(num - 60.0, -0.0755148492), 0.0, 255.0));
		double num4;
		if (num >= 66.0)
		{
			num4 = 255.0;
		}
		else
		{
			num4 = ((num <= 19.0) ? 0.0 : Math.Clamp(138.5177312231 * Math.Log(num - 10.0) - 305.0447927307, 0.0, 255.0));
		}
		return (R: (byte)num2, G: (byte)num3, B: (byte)num4);
	}
}
