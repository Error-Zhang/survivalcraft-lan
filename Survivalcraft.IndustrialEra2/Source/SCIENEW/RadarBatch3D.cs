using Engine;
using Engine.Graphics;

namespace SCIENEW;

public class RadarBatch3D : TexturedBatch3D
{
	public static RadarShader Shader = new RadarShader();

	public bool HighSample;

	public Vector2[] Points = new Vector2[8];

	public float Time;

	public new void Flush(Matrix matrix, bool clearAfterFlush = true)
	{
		Display.DepthStencilState = DepthStencilState;
		Display.RasterizerState = RasterizerState;
		Display.BlendState = BlendState;
		FlushWithCurrentState2(matrix, HighSample, clearAfterFlush);
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
