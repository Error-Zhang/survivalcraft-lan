using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class PerspectiveCamera : BasePerspectiveCamera
{
	public new Matrix m_projectionMatrix = Matrix.Zero;

	public RenderTarget2D ViewTexture = new RenderTarget2D(256, 256, 1, ColorFormat.Rgba8888, DepthFormat.Depth24Stencil8);

	public override bool UsesMovementControls => false;

	public override bool IsEntityControlEnabled => true;

	public override Matrix ProjectionMatrix => m_projectionMatrix;

	public override void Activate(Camera previousCamera)
	{
		SetupPerspectiveCamera(previousCamera.ViewPosition, previousCamera.ViewDirection, previousCamera.ViewUp);
	}

	public void SetViewMatrix(Matrix viewMatrix)
	{
		m_viewMatrix = null;
		SetupPerspectiveCamera(viewMatrix.Translation, viewMatrix.Forward, viewMatrix.Up);
	}

	public PerspectiveCamera(GameWidget gameWidget, Matrix viewMatrix, Matrix projectionMatrix)
		: base(gameWidget)
	{
		m_projectionMatrix = projectionMatrix;
		SetViewMatrix(viewMatrix);
	}

	public override void Update(float dt)
	{
	}
}
