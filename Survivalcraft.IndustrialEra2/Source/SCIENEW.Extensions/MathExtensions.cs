using System;
using Engine;

namespace SCIENEW.Extensions;

public static class MathExtensions
{
	public static float AngleDifference(this Vector2 v1, Vector2 v2)
	{
		float num = MathF.Atan2(v1.Y, v1.X);
		return MathF.Atan2(v2.Y, v2.X) - num;
	}
}
