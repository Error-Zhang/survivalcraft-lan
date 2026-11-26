using System;
using Engine;
using Engine.Graphics;
using SCIENEW;
using SCIENEW.AssetProtection;

namespace Game;

public class IndustryBackgroundWidget : Widget
{
	public Vector2 m_position;

	public IndustryBackgroundRenderer2D IndustryBackgroundRenderer2D = new IndustryBackgroundRenderer2D();

	public RenderTarget2D m_defaultRT = new RenderTarget2D(1920, 1080, 1, ColorFormat.Rgba8888, DepthFormat.Depth24Stencil8);

	public Texture2D Texture { get; set; }

	public bool IsStatic { get; set; }

	public IndustryBackgroundWidget()
	{
		Texture = ContentManager.Get<Texture2D>(AmpkContentRegistrar.IndustrialPanoramaPath, throwOnNotFound: false)
			?? ContentManager.Get<Texture2D>(PanoramaWidget.TexturePath);
		IndustryBackgroundBatch2D industryBackgroundBatch2D = IndustryBackgroundRenderer2D.FlatBatch(1);
		Vector2 zero = Vector2.Zero;
		Vector2 corner = new Vector2(m_defaultRT.Width, m_defaultRT.Height);
		industryBackgroundBatch2D.QueueQuad(zero, corner, 0f, Color.Transparent);
		industryBackgroundBatch2D.TransformLines(GlobalTransform, industryBackgroundBatch2D.TriangleVertices.Count);
		industryBackgroundBatch2D.TransformTriangles(GlobalTransform, industryBackgroundBatch2D.TriangleVertices.Count);
		RenderTarget2D renderTarget = Display.RenderTarget;
		Display.RenderTarget = m_defaultRT;
		industryBackgroundBatch2D.Flush();
		Display.RenderTarget = renderTarget;
	}

	public void DrawImage(DrawContext dc)
	{
		if (IsStatic)
		{
			float num = (float)MathUtils.Remainder(Time.FrameStartTime, 10000.0);
			float x = 2f * SimplexNoise.OctavedNoise(num, 0.02f, 4, 2f, 0.5f) - 1f;
			float y = 2f * SimplexNoise.OctavedNoise(num + 100f, 0.02f, 4, 2f, 0.5f) - 1f;
			m_position += 0.03f * new Vector2(x, y) * MathUtils.Min(Time.FrameDuration, 0.1f);
			m_position.X = MathUtils.Remainder(m_position.X, 1f);
			m_position.Y = MathUtils.Remainder(m_position.Y, 1f);
			float f = 0.5f * MathUtils.PowSign(MathF.Sin(0.21f * num + 2f), 2f) + 0.5f;
			float num2 = MathUtils.Lerp(0.13f, 0.3f, f);
			float num3 = num2 / (float)m_defaultRT.Height * (float)m_defaultRT.Width / ActualSize.X * ActualSize.Y;
			float x2 = m_position.X;
			float y2 = m_position.Y;
			Vector2 zero = Vector2.Zero;
			Vector2 actualSize = ActualSize;
			Vector2 texCoord = new Vector2(x2 - num2, y2 - num3);
			Vector2 texCoord2 = new Vector2(x2 + num2, y2 + num3);
			TexturedBatch2D texturedBatch2D = dc.PrimitivesRenderer2D.TexturedBatch(m_defaultRT, useAlphaTest: false, 0, DepthStencilState.DepthWrite, null, BlendState.AlphaBlend, SamplerState.LinearWrap);
			int count = texturedBatch2D.TriangleVertices.Count;
			texturedBatch2D.QueueQuad(zero, actualSize, 1f, texCoord, texCoord2, GlobalColorTransform);
			texturedBatch2D.TransformTriangles(GlobalTransform, count);
		}
		else
		{
			IndustryBackgroundBatch2D industryBackgroundBatch2D = IndustryBackgroundRenderer2D.FlatBatch(1);
			Vector2 zero2 = Vector2.Zero;
			Vector2 corner = GlobalBounds.Size();
			industryBackgroundBatch2D.QueueQuad(zero2, corner, 0f, Color.Transparent);
			industryBackgroundBatch2D.TransformLines(GlobalTransform, industryBackgroundBatch2D.TriangleVertices.Count);
			industryBackgroundBatch2D.TransformTriangles(GlobalTransform, industryBackgroundBatch2D.TriangleVertices.Count);
			industryBackgroundBatch2D.Flush();
		}
	}

	public void DrawSquares(DrawContext dc)
	{
		FlatBatch2D flatBatch2D = dc.PrimitivesRenderer2D.FlatBatch(1, DepthStencilState.None, null, BlendState.AlphaBlend);
		int count = flatBatch2D.LineVertices.Count;
		int count2 = flatBatch2D.TriangleVertices.Count;
		float num = (float)MathUtils.Remainder(Time.FrameStartTime, 10000.0);
		float num2 = ActualSize.X / 12f;
		float num3 = (float)(int)GlobalColorTransform.A / 255f;
		for (float num4 = 0f; num4 < ActualSize.X; num4 += num2)
		{
			for (float num5 = 0f; num5 < ActualSize.Y; num5 += num2)
			{
				float num6 = 0.35f * MathF.Pow(MathUtils.Saturate(SimplexNoise.OctavedNoise(num4 + 1000f, num5, 0.7f * num, 0.5f, 1, 2f, 1f) - 0.1f), 1f) * num3;
				float num7 = 0.7f * MathF.Pow(SimplexNoise.OctavedNoise(num4, num5, 0.5f * num, 0.5f, 1, 2f, 1f), 3f) * num3;
				Vector2 corner = new Vector2(num4, num5);
				Vector2 corner2 = new Vector2(num4 + num2, num5 + num2);
				if (num6 > 0.01f)
				{
					flatBatch2D.QueueRectangle(corner, corner2, 0f, new Color(0f, 0f, 0f, num6));
				}
				if (num7 > 0.01f)
				{
					flatBatch2D.QueueQuad(corner, corner2, 0f, new Color(0f, 0f, 0f, num7));
				}
			}
		}
		flatBatch2D.TransformLines(GlobalTransform, count);
		flatBatch2D.TransformTriangles(GlobalTransform, count2);
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		IsDrawRequired = true;
	}

	public override void Draw(DrawContext dc)
	{
		DrawImage(dc);
		DrawSquares(dc);
	}
}
