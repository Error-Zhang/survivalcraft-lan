using System;
using Engine;
using Game;
using SCIENEW.Extensions;

namespace SCIENEW;

public class TelescopeCamera_MoveView2 : BasePerspectiveCamera, IScalableCamera
{
	public Vector3 m_position;

	public static Vector3 m_direction;

	public Vector2 m_angles = new Vector2(0f, MathUtils.DegToRad(30f));

	public float m_distance = 10f;

	public override bool UsesMovementControls => true;

	public override bool IsEntityControlEnabled => true;

	public TelescopeCamera_MoveView2(GameWidget view)
		: base(view)
	{
	}

	public override void Activate(Camera previousCamera)
	{
		m_angles = new Vector2(0f, (float)Math.Asin(previousCamera.ViewDirection.Y));
		m_angles.X = (float)Math.Acos((double)previousCamera.ViewDirection.X / Math.Cos(m_angles.Y));
		if (previousCamera.ViewDirection.Z > 0f)
		{
			m_angles.X = 0f - m_angles.X;
		}
		SetupPerspectiveCamera(previousCamera.ViewPosition, previousCamera.ViewDirection, previousCamera.ViewUp);
	}

	public Vector3 GetDirection()
	{
		return m_direction;
	}

	public override void Update(float dt)
	{
		ComponentPlayer componentPlayer = GameWidget.PlayerData.ComponentPlayer;
		if (componentPlayer == null || GameWidget.Target == null)
		{
			return;
		}
		ComponentInput componentInput = componentPlayer.ComponentInput;
		if (Terrain.ExtractContents(componentPlayer.ComponentMiner.ActiveBlockValue) != AntitankSniperBlock.Index)
		{
			componentPlayer.GameWidget.ActiveCamera = new FppCamera(componentPlayer.GameWidget);
			return;
		}
		Vector3 cameraCrouchMove = componentInput.PlayerInput.CameraCrouchMove;
		Vector2 cameraLook = componentInput.PlayerInput.CameraLook;
		m_angles.X = MathUtils.NormalizeAngle(m_angles.X - 4f * cameraLook.X * dt + 0.5f * cameraCrouchMove.X * dt);
		m_angles.Y = (float)Math.Clamp(MathUtils.NormalizeAngle((double)m_angles.Y + 4.0 * (double)cameraLook.Y * (double)dt), MathUtils.DegToRad(-45f), MathUtils.DegToRad(80f));
		m_distance = Math.Clamp(m_distance + 50f * cameraCrouchMove.Z * dt, 2f, 100f);
		Vector3 vector = new Vector3(m_distance, 0f, 0f).Transform(Matrix.CreateFromYawPitchRoll(m_angles.X, 0f, m_angles.Y));
		Vector3 vector2 = GameWidget.Target.ComponentBody.BoundingBox.Center() + new Vector3(0f, 0.5f, 0f);
		Vector3 vector3 = vector2 + vector;
		if (vector3.Distance(m_position) < 100f)
		{
			Vector3 vector4 = vector3 - m_position;
			float num = MathUtils.Saturate(10f * dt);
			m_position += num * vector4;
		}
		else
		{
			m_position = vector3;
		}
		Vector3 vector5 = m_position - vector2;
		float? num2 = null;
		Vector3 vector6 = vector5.Cross(Vector3.UnitY).Normalize();
		Vector3 vector7 = vector5.Cross(vector6).Normalize();
		for (int i = 0; i <= 0; i++)
		{
			for (int j = 0; j <= 0; j++)
			{
				Vector3 vector8 = 0.5f * (vector6 * i + vector7 * j);
				Vector3 vector9 = vector2 + vector8;
				Vector3 end = vector9 + vector5 + vector5.Normalize() * 0.5f;
				TerrainRaycastResult? terrainRaycastResult = componentPlayer.m_subsystemTerrain.Raycast(vector9, end, useInteractionBoxes: false, skipAirBlocks: true, (int value, float distance) => Terrain.ExtractContents(value) != 0);
				if (terrainRaycastResult.HasValue)
				{
					num2 = (num2.HasValue ? MathUtils.Min(num2.Value, terrainRaycastResult.Value.Distance) : terrainRaycastResult.Value.Distance);
				}
			}
		}
		Vector3 vector10 = (num2.HasValue ? (vector2 + vector5.Normalize() * MathUtils.Max(num2.Value - 0.5f, 0.2f)) : (vector2 + vector5));
		m_direction = (vector10 - vector2).Normalize() * m_distance;
		SetupPerspectiveCamera(vector10 + new Vector3(0f, 0.15f, 0f), vector10 - vector2, Vector3.UnitY);
	}
}
