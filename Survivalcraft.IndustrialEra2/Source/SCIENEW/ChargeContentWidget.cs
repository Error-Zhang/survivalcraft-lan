using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class ChargeContentWidget : CanvasWidget
{
	public float m_relativeVolume;

	public Texture2D m_texture = ContentManager.Get<Texture2D>("Textures/White");

	public override void Draw(DrawContext dc)
	{
		base.Draw(dc);
		if (!(m_relativeVolume <= 0f))
		{
			Matrix m = GlobalTransform;
			Vector2 v = Vector2.Zero;
			Vector2 v2 = new Vector2(ActualSize.X, 0f);
			Vector2 v3 = ActualSize;
			Vector2 v4 = new Vector2(0f, ActualSize.Y);
			Vector2 zero = Vector2.Zero;
			float num = 1f / 3f;
			TexturedBatch2D texturedBatch2D = dc.PrimitivesRenderer2D.TexturedBatch(m_texture, useAlphaTest: true, 1, DepthStencilState.None, null, null, SamplerState.PointWrap);
			v += new Vector2(0f, (1f - m_relativeVolume) / 1f * Size.Y);
			v2 += new Vector2(0f, (1f - m_relativeVolume) / 1f * Size.Y);
			Vector2.Transform(ref v, ref m, out var result);
			Vector2.Transform(ref v2, ref m, out var result2);
			Vector2.Transform(ref v3, ref m, out var result3);
			Vector2.Transform(ref v4, ref m, out var result4);
			Color color = new Color(0, 255, 255) * GlobalColorTransform;
			if (color.A != 0)
			{
				float num2 = ActualSize.Y * m_relativeVolume / ActualSize.X;
				Vector2 texCoord = zero;
				Vector2 texCoord2 = zero + Vector2.UnitX * num;
				Vector2 texCoord3 = zero + Vector2.One * new Vector2(num, num * num2);
				Vector2 texCoord4 = zero + Vector2.UnitY * num * num2;
				Vector2 zero2 = Vector2.Zero;
				texCoord += zero2;
				texCoord2 += zero2;
				texCoord3 += zero2;
				texCoord4 += zero2;
				texturedBatch2D.QueueQuad(result, result2, result3, result4, 0f, texCoord, texCoord2, texCoord3, texCoord4, color);
			}
		}
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		IsDrawRequired = true;
		DesiredSize = Size;
	}
}
