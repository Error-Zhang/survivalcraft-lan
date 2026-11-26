using System;
using Engine;
using Engine.Graphics;

namespace SCIENEW;

public class TechTreeBackgroundBatch2D : FlatBatch2D
{
	public static TechTreeBackgroundShader Shader = new TechTreeBackgroundShader();

	public new void Flush(bool clearAfterFlush = true)
	{
		Display.DepthStencilState = DepthStencilState;
		Display.RasterizerState = RasterizerState;
		Display.BlendState = BlendState;
		FlushWithCurrentState2(clearAfterFlush);
	}

	public void FlushWithCurrentState2(bool clearAfterFlush = true)
	{
		Shader.RenderSize = ((Display.RenderTarget == null) ? new Vector2(1024f) : new Vector2(Display.RenderTarget.Width, Display.RenderTarget.Height));
		Shader.Time = (float)(DateTime.Now.ToOADate() % 1.0 * 24.0 * 60.0 * 60.0 % 600.0);
		Shader.Transforms.World[0] = PrimitivesRenderer2D.ViewportMatrix();
		FlushWithDeviceState(Shader, clearAfterFlush);
	}
}
