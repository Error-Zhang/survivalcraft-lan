using Engine;
using Engine.Graphics;

namespace SCIENEW;

public class RadarBatch2D : TexturedBatch2D
{
	public static RadarShader Shader = new RadarShader();

	public bool HighSample;

	public Vector2[] Points = new Vector2[8];

	public float Time;

	public new void Flush(bool clearAfterFlush = true)
	{
		Display.DepthStencilState = DepthStencilState;
		Display.RasterizerState = RasterizerState;
		Display.BlendState = BlendState;
		FlushWithCurrentState2(ViewportMatrix(), HighSample, clearAfterFlush);
	}

	public static Matrix ViewportMatrix()
	{
		Viewport viewport = Display.Viewport;
		return new Matrix(2f * (1f / (float)viewport.Width), 0f, 0f, 0f, 0f, -2f * (1f / (float)viewport.Height), 0f, 0f, 0f, 0f, 1f, 0f, -1f, 1f, 0f, 1f);
	}

	public void FlushWithCurrentState2(Matrix matrix, bool highSample, bool clearAfterFlush = true)
	{
		Shader.Transforms.World[0] = matrix;
		Shader.Time = Time;
		for (int i = 0; i < 8; i++)
		{
			Shader.m_pointsPosition[i] = ((i < Points.Length) ? (Points[i] / 2f) : Vector2.Zero);
		}
		Shader.RenderSize = (highSample ? new Vector2(256f, 256f) : new Vector2(64f, 64f));
		FlushWithDeviceState(Shader, clearAfterFlush);
	}
}
