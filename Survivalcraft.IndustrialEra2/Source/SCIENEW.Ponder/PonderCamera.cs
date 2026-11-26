using System;
using Engine;
using Game;

namespace SCIENEW.Ponder;

public class PonderCamera : Camera
{
	public Matrix m_viewMatrix;

	public Matrix m_projectionMatrix;

	public Matrix m_viewportMatrix;

	public Vector3 CameraPosition;

	public Vector3 CameraDirection;

	public Vector3 CameraUp;

	public Vector3 CameraRight;

	public override Vector3 ViewPosition => CameraPosition;

	public override Vector3 ViewDirection => CameraDirection;

	public override Vector3 ViewUp => CameraUp;

	public override Vector3 ViewRight => CameraRight;

	public override Matrix ViewMatrix => m_viewMatrix;

	public override Matrix InvertedViewMatrix => Matrix.Invert(m_viewMatrix);

	public override Matrix ProjectionMatrix => m_projectionMatrix;

	public override Matrix ScreenProjectionMatrix => m_projectionMatrix;

	public override Matrix InvertedProjectionMatrix => Matrix.Invert(m_projectionMatrix);

	public override Matrix ViewProjectionMatrix => m_viewMatrix * m_projectionMatrix * m_viewportMatrix;

	public override Vector2 ViewportSize
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override Matrix ViewportMatrix => m_viewportMatrix;

	public override BoundingFrustum ViewFrustum => new BoundingFrustum(ViewProjectionMatrix);

	public override bool UsesMovementControls => false;

	public override bool IsEntityControlEnabled => false;

	public PonderCamera(GameWidget gameWidget)
		: base(gameWidget)
	{
	}

	public override void Update(float dt)
	{
	}
}
