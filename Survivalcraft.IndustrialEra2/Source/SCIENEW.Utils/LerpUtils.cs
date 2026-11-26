using System;
using System.Runtime.CompilerServices;
using Engine;

namespace SCIENEW.Utils;

public static class LerpUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Map(float fromMin, float fromMax, float toMin, float toMax, float f)
	{
		float f2 = (f - fromMin) / (fromMax - fromMin);
		return MathUtils.Lerp(toMin, toMax, f2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float MapClamp(float fromMin, float fromMax, float toMin, float toMax, float f)
	{
		float value = (f - fromMin) / (fromMax - fromMin);
		value = Math.Clamp(value, 0f, 1f);
		return MathUtils.Lerp(toMin, toMax, value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Map(double fromMin, double fromMax, double toMin, double toMax, double f)
	{
		double f2 = (f - fromMin) / (fromMax - fromMin);
		return MathUtils.Lerp(toMin, toMax, f2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double MapClamp(double fromMin, double fromMax, double toMin, double toMax, double f)
	{
		double value = (f - fromMin) / (fromMax - fromMin);
		value = Math.Clamp(value, 0.0, 1.0);
		return MathUtils.Lerp(toMin, toMax, value);
	}
}
