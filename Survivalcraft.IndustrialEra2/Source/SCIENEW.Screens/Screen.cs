using System;
using Engine;
using Engine.Graphics;
using SCIENEW.Extensions;

namespace SCIENEW.Screens;

public struct Screen
{
	public static readonly RasterizerState QuadRasterizerState = RasterizerState.CullClockwiseScissor;

	public static readonly RasterizerState FontRasterizerState = RasterizerState.CullCounterClockwiseScissor;

	public Vector3 WorldPos1;

	public Vector3 WorldPos2;

	public Vector3 WorldPos3;

	public Vector3 WorldPos4;

	public Vector3 Corner => WorldPos1;

	public Vector3 Right => WorldPos2 - WorldPos1;

	public Vector3 Up => WorldPos4 - WorldPos1;

	public Vector3 CenterPos => (WorldPos1 + WorldPos2 + WorldPos3 + WorldPos4) / 4f;

	public float VisibleRadius => MathF.Max((WorldPos1 - CenterPos).Length(), MathF.Max((WorldPos2 - CenterPos).Length(), MathF.Max((WorldPos3 - CenterPos).Length(), (WorldPos4 - CenterPos).Length())));

	public Vector3 Normal => Right.Cross(Up).Normalize();

	public Screen(Vector3 worldPos1, Vector3 worldPos2, Vector3 worldPos3, Vector3 worldPos4)
	{
		WorldPos1 = worldPos1;
		WorldPos2 = worldPos2;
		WorldPos3 = worldPos3;
		WorldPos4 = worldPos4;
	}

	public static Screen FromQuad(Vector3 corner, Vector3 right, Vector3 up)
	{
		return new Screen(corner, corner + right, corner + right + up, corner + up);
	}

	public void GetTextAxes(float scale, out Vector3 right, out Vector3 down)
	{
		right = ((Right.LengthSquared() > 0f) ? (Right.Normalize() * scale) : Vector3.Zero);
		down = ((Up.LengthSquared() > 0f) ? (-Up.Normalize() * scale) : Vector3.Zero);
	}
}
