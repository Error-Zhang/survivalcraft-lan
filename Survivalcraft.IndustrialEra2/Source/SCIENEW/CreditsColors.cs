using System;
using Engine;

namespace SCIENEW;

public static class CreditsColors
{
	public static Color RainbowMixWhite(float timeSeconds, float cycleSeconds = 4f)
	{
		Color c = HsvToRgb(timeSeconds / Math.Max(0.01f, cycleSeconds) % 1f, 1f, 1f);
		return Color.Lerp(Color.White, c, 0.5f);
	}

	public static Color ForRole(string role, float timeSeconds)
	{
		if (role != null && role.Equals("developer", StringComparison.OrdinalIgnoreCase))
		{
			return RainbowMixWhite(timeSeconds);
		}
		return Color.White;
	}

	private static Color HsvToRgb(float h, float s, float v)
	{
		float num = v * s;
		float num2 = num * (1f - MathF.Abs(h * 6f % 2f - 1f));
		float num3 = v - num;
		float num4 = h * 6f;
		float num5;
		float num6;
		float num7;
		if (num4 < 1f)
		{
			num5 = num;
			num6 = num2;
			num7 = 0f;
		}
		else if (num4 < 2f)
		{
			num5 = num2;
			num6 = num;
			num7 = 0f;
		}
		else if (num4 < 3f)
		{
			num5 = 0f;
			num6 = num;
			num7 = num2;
		}
		else if (num4 < 4f)
		{
			num5 = 0f;
			num6 = num2;
			num7 = num;
		}
		else if (num4 < 5f)
		{
			num5 = num2;
			num6 = 0f;
			num7 = num;
		}
		else
		{
			num5 = num;
			num6 = 0f;
			num7 = num2;
		}
		return new Color((int)((num5 + num3) * 255f), (int)((num6 + num3) * 255f), (int)((num7 + num3) * 255f));
	}
}
