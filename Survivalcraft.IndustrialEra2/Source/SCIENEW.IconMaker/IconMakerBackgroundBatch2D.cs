using Engine;
using Engine.Graphics;

namespace SCIENEW.IconMaker;

public class IconMakerBackgroundBatch2D : TexturedBatch2D
{
	public static IconMakerBackgroundShader Shader = new IconMakerBackgroundShader();

	public new void Flush(bool clearAfterFlush = true)
	{
		Display.DepthStencilState = DepthStencilState;
		Display.RasterizerState = RasterizerState;
		Display.BlendState = BlendState;
		FlushWithDeviceState(Shader, clearAfterFlush);
	}

	public void FlushWithProject(IconMakerProject project, Vector2 outputSize, Matrix matrix, bool clearAfterFlush = true)
	{
		Shader.Transforms.World[0] = matrix;
		Shader.SetTexture(Texture, SamplerState);
		Shader.ApplyProject(project, outputSize);
		Flush(clearAfterFlush);
	}
}
