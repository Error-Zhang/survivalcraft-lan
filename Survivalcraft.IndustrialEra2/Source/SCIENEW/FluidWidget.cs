using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class FluidWidget : CanvasWidget
{
	public int m_fluidValue;

	public float m_relativeVolume;

	public override void Draw(DrawContext dc)
	{
		base.Draw(dc);
		Fluid fluid = FluidsManager.GetFluid(m_fluidValue);
		if (fluid == null || m_relativeVolume <= 0f)
		{
			return;
		}
		Matrix m = GlobalTransform;
		Vector2 v = Vector2.Zero;
		Vector2 v2 = new Vector2(ActualSize.X, 0f);
		Vector2 v3 = ActualSize;
		Vector2 v4 = new Vector2(0f, ActualSize.Y);
		Vector2 zero = Vector2.Zero;
		float num = 1f / (float)fluid.GetTextureSubdivision(m_fluidValue);
		TexturedBatch2D texturedBatch2D = dc.PrimitivesRenderer2D.TexturedBatch(fluid.GetTexture(m_fluidValue), useAlphaTest: true, 1, DepthStencilState.None, null, null, SamplerState.PointWrap);
		if (fluid.GetFluidType(m_fluidValue) == FluidType.Liquid)
		{
			v += new Vector2(0f, (1f - m_relativeVolume) / 1f * Size.Y);
			v2 += new Vector2(0f, (1f - m_relativeVolume) / 1f * Size.Y);
			Vector2.Transform(ref v, ref m, out var result);
			Vector2.Transform(ref v2, ref m, out var result2);
			Vector2.Transform(ref v3, ref m, out var result3);
			Vector2.Transform(ref v4, ref m, out var result4);
			Color color = fluid.GetColor(m_fluidValue) * GlobalColorTransform;
			if (color.A != 0)
			{
				float num2 = ActualSize.Y * m_relativeVolume / ActualSize.X;
				Vector2 texCoord = zero;
				Vector2 texCoord2 = zero + Vector2.UnitX * num;
				Vector2 texCoord3 = zero + Vector2.One * new Vector2(num, num * num2);
				Vector2 texCoord4 = zero + Vector2.UnitY * num * num2;
				int textureSlot = fluid.GetTextureSlot(m_fluidValue);
				Vector2 vector = new Vector2(num * (float)(textureSlot % fluid.GetTextureSubdivision(m_fluidValue)), num * (float)(textureSlot / fluid.GetTextureSubdivision(m_fluidValue)));
				texCoord += vector;
				texCoord2 += vector;
				texCoord3 += vector;
				texCoord4 += vector;
				texturedBatch2D.QueueQuad(result, result2, result3, result4, 0f, texCoord, texCoord2, texCoord3, texCoord4, color);
			}
		}
		else if (fluid.GetFluidType(m_fluidValue) == FluidType.Gas)
		{
			Vector2.Transform(ref v, ref m, out var result5);
			Vector2.Transform(ref v2, ref m, out var result6);
			Vector2.Transform(ref v3, ref m, out var result7);
			Vector2.Transform(ref v4, ref m, out var result8);
			Color c = fluid.GetColor(m_fluidValue) * GlobalColorTransform;
			float f = (1f - m_relativeVolume) / 1f;
			c = Color.Lerp(c, Color.Transparent, f);
			if (c.A != 0)
			{
				float num3 = ActualSize.Y / ActualSize.X;
				Vector2 texCoord5 = zero;
				Vector2 texCoord6 = zero + Vector2.UnitX * num;
				Vector2 texCoord7 = zero + Vector2.One * new Vector2(num, num * num3);
				Vector2 texCoord8 = zero + Vector2.UnitY * num * num3;
				int textureSlot2 = fluid.GetTextureSlot(m_fluidValue);
				Vector2 vector2 = new Vector2(num * (float)(textureSlot2 % fluid.GetTextureSubdivision(m_fluidValue)), num * (float)(textureSlot2 / fluid.GetTextureSubdivision(m_fluidValue)));
				texCoord5 += vector2;
				texCoord6 += vector2;
				texCoord7 += vector2;
				texCoord8 += vector2;
				texturedBatch2D.QueueQuad(result5, result6, result7, result8, 0f, texCoord5, texCoord6, texCoord7, texCoord8, c);
			}
		}
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		IsDrawRequired = true;
		DesiredSize = Size;
	}
}
