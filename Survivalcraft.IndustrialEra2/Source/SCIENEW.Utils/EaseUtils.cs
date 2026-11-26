using System;
using Engine;

namespace SCIENEW.Utils;

public static class EaseUtils
{
	public static float BezierEaseInOut(float start, float target, float t, float handleLength = 0.3f)
	{
		float num = 1f - t;
		float num2 = num * num * num;
		float num3 = 3f * num * num * t;
		float num4 = 3f * num * t * t;
		float num5 = t * t * t;
		return num2 * start + num3 * start + num4 * target + num5 * target;
	}

	public static float ExpEaseOut(float start, float target, float t, float exp = 3f)
	{
		float f = MathF.Pow(1f - t, exp);
		return MathUtils.Lerp(start, target, f);
	}

	public static float ExpEaseIn(float start, float target, float t, float exp = 3f)
	{
		float f = MathF.Pow(t, exp);
		return MathUtils.Lerp(start, target, f);
	}

	public static float ExpEaseInOut(float start, float target, float t, float exp = 3f)
	{
		if (t < 0.5f)
		{
			float f = MathF.Pow(t * 2f, exp) * 0.5f;
			return MathUtils.Lerp(start, target, f);
		}
		float x = 1f - (t - 0.5f) * 2f;
		float f2 = 1f - MathF.Pow(x, exp) * 0.5f;
		return MathUtils.Lerp(start, target, f2);
	}
}
