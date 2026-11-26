using System;
using Engine;
using Game;
using SCIENEW.Extensions;

namespace SCIENEW.Screens;

public static class ScreenGeometry
{
	private readonly struct FaceBasis(Vector3 outward, Vector3 unitRight, Vector3 unitUp)
	{
		public readonly Vector3 Outward = outward;

		public readonly Vector3 UnitRight = unitRight;

		public readonly Vector3 UnitUp = unitUp;
	}

	private const float CornerEpsilon = 0.0001f;

	private static readonly FaceBasis[] FaceBases = new FaceBasis[6]
	{
		new FaceBasis(new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f)),
		new FaceBasis(new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, -1f), new Vector3(0f, 1f, 0f)),
		new FaceBasis(new Vector3(0f, 0f, -1f), new Vector3(-1f, 0f, 0f), new Vector3(0f, 1f, 0f)),
		new FaceBasis(new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 0f)),
		new FaceBasis(new Vector3(0f, 1f, 0f), new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, -1f)),
		new FaceBasis(new Vector3(0f, -1f, 0f), new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f))
	};

	public static void GetMountFaceBasis(int face, out Vector3 unitRight, out Vector3 unitUp, out Vector3 outward)
	{
		ref FaceBasis reference = ref FaceBases[face];
		outward = reference.Outward;
		unitRight = reference.UnitRight;
		unitUp = reference.UnitUp;
	}

	public static Screen FromMountFace(int face, Vector3 center, Vector2 size, float normalOffset = 0f)
	{
		ref FaceBasis reference = ref FaceBases[face];
		Vector3 vector = reference.UnitRight * size.X;
		Vector3 vector2 = reference.UnitUp * size.Y;
		return Screen.FromQuad(center - vector * 0.5f - vector2 * 0.5f + reference.Outward * normalOffset, vector, vector2);
	}

	public static Point3 FlatCellToBlock(int face, Point3 anchor, int flatX, int flatY)
	{
		switch (face)
		{
		case 0:
		case 2:
			return new Point3(flatX, flatY, anchor.Z);
		case 1:
		case 3:
			return new Point3(anchor.X, flatX, flatY);
		default:
			return new Point3(flatX, anchor.Y, flatY);
		}
	}

	public static Screen FromFlatInscribedRect(int face, Point3 anchor, int topLeftFlatX, int topLeftFlatY, float spanFlatU, float spanFlatV, float normalOffset, float borderInset)
	{
		ref FaceBasis reference = ref FaceBases[face];
		Point3 point = FlatCellToBlock(face, Point3.Zero, 0, 0);
		Point3 point2 = FlatCellToBlock(face, Point3.Zero, 1, 0);
		Point3 point3 = FlatCellToBlock(face, Point3.Zero, 0, 1);
		Vector3 vector = new Vector3(point2.X - point.X, point2.Y - point.Y, point2.Z - point.Z);
		Vector3 vector2 = new Vector3(point3.X - point.X, point3.Y - point.Y, point3.Z - point.Z);
		Vector3 vector3 = new Vector3(FlatCellToBlock(face, anchor, topLeftFlatX, topLeftFlatY)) + new Vector3(0.5f) - vector * 0.5f - vector2 * 0.5f + reference.Outward * normalOffset;
		vector3 += vector * borderInset + vector2 * borderInset;
		float num = spanFlatU - 2f * borderInset;
		float num2 = spanFlatV - 2f * borderInset;
		_003C_003Ey__InlineArray4<Vector3> buffer = default;
		buffer[0] = vector3;
		buffer[1] = vector3 + vector * num;
		buffer[2] = vector3 + vector2 * num2;
		buffer[3] = vector3 + vector * num + vector2 * num2;
		return AxisAlignedFromParallelogram(buffer, reference.UnitRight, reference.UnitUp);
	}

	private static Screen AxisAlignedFromParallelogram(ReadOnlySpan<Vector3> corners, Vector3 unitRight, Vector3 unitUp)
	{
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		ReadOnlySpan<Vector3> readOnlySpan = corners;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			Vector3 v = readOnlySpan[i];
			float y = v.Dot(unitRight);
			float y2 = v.Dot(unitUp);
			num = MathF.Min(num, y);
			num2 = MathF.Max(num2, y);
			num3 = MathF.Min(num3, y2);
			num4 = MathF.Max(num4, y2);
		}
		Vector3 vector = PickCorner(corners, unitRight, unitUp, num, num3);
		Vector3 vector2 = PickCorner(corners, unitRight, unitUp, num2, num3);
		Vector3 vector3 = PickCorner(corners, unitRight, unitUp, num, num4);
		return Screen.FromQuad(vector, vector2 - vector, vector3 - vector);
	}

	private static Vector3 PickCorner(ReadOnlySpan<Vector3> corners, Vector3 unitRight, Vector3 unitUp, float targetR, float targetU)
	{
		ReadOnlySpan<Vector3> readOnlySpan = corners;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			Vector3 vector = readOnlySpan[i];
			if (MathF.Abs(vector.Dot(unitRight) - targetR) <= 0.0001f && MathF.Abs(vector.Dot(unitUp) - targetU) <= 0.0001f)
			{
				return vector;
			}
		}
		throw new InvalidOperationException("Flat rectangle corners do not match display axes.");
	}

	public static bool TryComputeViewParallaxBias(Vector3 quadCenter, Camera camera, out Vector3 bias)
	{
		Vector3 vector = quadCenter - camera.ViewPosition;
		float num = vector.Dot(camera.ViewDirection);
		if (num <= 0.01f)
		{
			bias = Vector3.Zero;
			return false;
		}
		float num2 = vector.Length();
		bias = (0f - (0.01f + 0.02f * num)) / num2 * vector;
		return true;
	}

	public static Screen ApplyParallaxBias(Screen screen, Vector3 bias)
	{
		return Screen.FromQuad(screen.Corner + bias, screen.Right, screen.Up);
	}
}
