using System;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions;

namespace SCIENEW;

public class TelescopeCamera_ScaleView : BasePerspectiveCamera, IScalableCamera
{
	public Vector3 CameraPosition;

	public static Vector3 CameraDirection;

	public Vector2 CameraAngles = new Vector2(0f, MathUtils.DegToRad(30f));

	public float CameraScale = 0.5f;

	public override bool UsesMovementControls => true;

	public override bool IsEntityControlEnabled => true;

	public override Matrix ProjectionMatrix
	{
		get
		{
			m_projectionMatrix = CalculateBaseProjectionMatrix();
			ViewWidget viewWidget = GameWidget.ViewWidget;
			if (!viewWidget.ScalingRenderTargetSize.HasValue)
			{
				m_projectionMatrix *= MatrixUtils.CreateScaleTranslation(CameraScale * viewWidget.ActualSize.X, (0f - CameraScale) * viewWidget.ActualSize.Y, viewWidget.ActualSize.X / 2f, viewWidget.ActualSize.Y / 2f) * viewWidget.GlobalTransform * MatrixUtils.CreateScaleTranslation(2f / (float)Display.Viewport.Width, -2f / (float)Display.Viewport.Height, -1f, 1f);
			}
			return m_projectionMatrix.Value;
		}
	}

	public TelescopeCamera_ScaleView(GameWidget view)
		: base(view)
	{
	}

	public override void Activate(Camera previousCamera)
	{
		CameraAngles = new Vector2(0f, (float)Math.Asin(previousCamera.ViewDirection.Y));
		CameraAngles.X = (float)Math.Acos((double)previousCamera.ViewDirection.X / Math.Cos(CameraAngles.Y));
		if (previousCamera.ViewDirection.Z > 0f)
		{
			CameraAngles.X = 0f - CameraAngles.X;
		}
		SetupPerspectiveCamera(previousCamera.ViewPosition, previousCamera.ViewDirection, previousCamera.ViewUp);
	}

	public Vector3 GetDirection()
	{
		return CameraDirection;
	}

	public override void Update(float dt)
	{
		ComponentPlayer componentPlayer = GameWidget.PlayerData.ComponentPlayer;
		if (componentPlayer == null || GameWidget.Target == null)
		{
			return;
		}
		ComponentInput componentInput = componentPlayer.ComponentInput;
		if (Terrain.ExtractContents(componentPlayer.ComponentMiner.ActiveBlockValue) != SniperBlock.Index)
		{
			componentPlayer.GameWidget.ActiveCamera = new FppCamera(componentPlayer.GameWidget);
			return;
		}
		Vector2 cameraLook = componentInput.PlayerInput.CameraLook;
		Vector3 cameraCrouchMove = componentInput.PlayerInput.CameraCrouchMove;
		CameraAngles.X = MathUtils.NormalizeAngle(CameraAngles.X - 4f * cameraLook.X * dt);
		CameraAngles.Y = Math.Clamp(CameraAngles.Y + 4f * cameraLook.Y * dt, MathUtils.DegToRad(-45f), MathUtils.DegToRad(80f));
		CameraScale = Math.Clamp(CameraScale + cameraCrouchMove.Z * dt, 0.5f, 10f);
		Vector3 vector = new Vector3(0.5f, 0f, 0f).Transform(Matrix.CreateFromYawPitchRoll(CameraAngles.X, 0f, CameraAngles.Y));
		Vector3 vector2 = GameWidget.Target.ComponentBody.BoundingBox.Center() + new Vector3(0f, 0.5f, 0f);
		CameraPosition = vector2 + vector;
		CameraDirection = Vector3.Normalize(CameraPosition - vector2);
		if (componentPlayer.m_subsystemTerrain.Raycast(CameraPosition, CameraPosition + 1f * CameraDirection, useInteractionBoxes: false, skipAirBlocks: true, (int value, float distance) => Terrain.ExtractContents(value) != 0).HasValue)
		{
			componentPlayer.GameWidget.ActiveCamera = new FppCamera(componentPlayer.GameWidget);
		}
		else
		{
			SetupPerspectiveCamera(CameraPosition + new Vector3(0f, 0.15f, 0f), CameraDirection, Vector3.UnitY);
		}
	}
}
