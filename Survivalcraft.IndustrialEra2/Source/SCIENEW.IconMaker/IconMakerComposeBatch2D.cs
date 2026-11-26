using Engine;
using Engine.Graphics;

namespace SCIENEW.IconMaker;

public class IconMakerComposeBatch2D : TexturedBatch2D
{
	public static IconMakerComposeShader Shader = new IconMakerComposeShader();

	public new void Flush(bool clearAfterFlush = true)
	{
		Display.DepthStencilState = DepthStencilState;
		Display.RasterizerState = RasterizerState;
		Display.BlendState = BlendState;
		FlushWithDeviceState(Shader, clearAfterFlush);
	}

	public void FlushWithProject(IconMakerProject project, Vector2 outputSize, Texture2D background, Texture2D subject, Matrix matrix, bool clearAfterFlush = true)
	{
		Shader.Transforms.World[0] = matrix;
		Shader.ApplyProject(project, outputSize, background, subject);
		Flush(clearAfterFlush);
	}
}
