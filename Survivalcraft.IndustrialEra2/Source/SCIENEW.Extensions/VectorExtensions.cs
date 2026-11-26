using System;
using System.Runtime.CompilerServices;
using Engine;

namespace SCIENEW.Extensions;

public static class VectorExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(this Vector3 v1, Vector3 v2)
	{
		return Vector3.Distance(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSquared(this Vector3 v1, Vector3 v2)
	{
		return Vector3.DistanceSquared(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Dot(this Vector3 v1, Vector3 v2)
	{
		return Vector3.Dot(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Cross(this Vector3 v1, Vector3 v2)
	{
		return Vector3.Cross(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Transform(this Vector3 v, Quaternion q)
	{
		return Vector3.Transform(v, q);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Transform(this Vector3 v, Matrix m)
	{
		return Vector3.Transform(v, m);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 TransformNormal(this Vector3 v, Matrix m)
	{
		return Vector3.TransformNormal(v, m);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Reflect(this Vector3 inDirection, Vector3 inNormal)
	{
		float num = -2f * Vector3.Dot(inNormal, inDirection);
		return new Vector3(num * inNormal.X + inDirection.X, num * inNormal.Y + inDirection.Y, num * inNormal.Z + inDirection.Z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Normalize(this Vector3 vector)
	{
		return Vector3.Normalize(vector);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 LimitLength(this Vector3 v, float maxLength)
	{
		return Vector3.LimitLength(v, maxLength);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Lerp(this Vector3 v1, Vector3 v2, float t)
	{
		return Vector3.Lerp(v1, v2, t);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 CatmullRom(this Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float t)
	{
		return Vector3.CatmullRom(v1, v2, v3, v4, t);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Floor(this Vector3 v)
	{
		return Vector3.Floor(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Ceiling(this Vector3 v)
	{
		return Vector3.Ceiling(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Round(this Vector3 v)
	{
		return Vector3.Round(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Min(this Vector3 v, float f)
	{
		return Vector3.Min(v, f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Min(this Vector3 v1, Vector3 v2)
	{
		return Vector3.Min(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Max(this Vector3 v, float f)
	{
		return Vector3.Max(v, f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Max(this Vector3 v1, Vector3 v2)
	{
		return Vector3.Max(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Clamp(this Vector3 v, float min, float max)
	{
		return Vector3.Clamp(v, min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Saturate(this Vector3 v)
	{
		return Vector3.Saturate(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 FixNaN(this Vector3 v)
	{
		if (float.IsNaN(v.X))
		{
			v.X = 0f;
		}
		if (float.IsNaN(v.Y))
		{
			v.Y = 0f;
		}
		if (float.IsNaN(v.Z))
		{
			v.Z = 0f;
		}
		return v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Slerp(this Vector3 a, Vector3 b, float t)
	{
		Vector3 vector = a.Normalize();
		Vector3 vector2 = b.Normalize();
		float num = (float)Math.Acos(Math.Clamp(Vector3.Dot(vector, vector2), -1f, 1f));
		float num2 = (float)Math.Sin(num);
		if (num2 < 1E-05f)
		{
			return Vector3.Lerp(vector, vector2, t).Normalize();
		}
		float num3 = (float)Math.Sin((1f - t) * num) / num2;
		float num4 = (float)Math.Sin(t * num) / num2;
		return (vector * num3 + vector2 * num4).Normalize();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(this Vector2 v1, Vector2 v2)
	{
		return Vector2.Distance(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSquared(this Vector2 v1, Vector2 v2)
	{
		return Vector2.DistanceSquared(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Dot(this Vector2 v1, Vector2 v2)
	{
		return Vector2.Dot(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Cross(this Vector2 v1, Vector2 v2)
	{
		return Vector2.Cross(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Rotate(this Vector2 v, float angle)
	{
		return Vector2.Rotate(v, angle);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Perpendicular(this Vector2 v)
	{
		return Vector2.Perpendicular(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float AngleTo(this Vector2 v1, Vector2 v2)
	{
		return Vector2.Angle(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Transform(this Vector2 v, Quaternion q)
	{
		return Vector2.Transform(v, q);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Transform(this Vector2 v, Matrix m)
	{
		return Vector2.Transform(v, m);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 TransformNormal(this Vector2 v, Matrix m)
	{
		return Vector2.TransformNormal(v, m);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Normalize(this Vector2 v)
	{
		return Vector2.Normalize(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 LimitLength(this Vector2 v, float maxLength)
	{
		return Vector2.LimitLength(v, maxLength);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Lerp(this Vector2 v1, Vector2 v2, float t)
	{
		return Vector2.Lerp(v1, v2, t);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 CatmullRom(this Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, float t)
	{
		return Vector2.CatmullRom(v1, v2, v3, v4, t);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Min(this Vector2 v, float f)
	{
		return Vector2.Min(v, f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Min(this Vector2 v1, Vector2 v2)
	{
		return Vector2.Min(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Max(this Vector2 v, float f)
	{
		return Vector2.Max(v, f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Max(this Vector2 v1, Vector2 v2)
	{
		return Vector2.Max(v1, v2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Clamp(this Vector2 v, float min, float max)
	{
		return Vector2.Clamp(v, min, max);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Saturate(this Vector2 v)
	{
		return Vector2.Saturate(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Floor(this Vector2 v)
	{
		return Vector2.Floor(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Ceiling(this Vector2 v)
	{
		return Vector2.Ceiling(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Round(this Vector2 v)
	{
		return Vector2.Round(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 FixNaN(this Vector2 v)
	{
		if (float.IsNaN(v.X))
		{
			v.X = 0f;
		}
		if (float.IsNaN(v.Y))
		{
			v.Y = 0f;
		}
		return v;
	}
}
