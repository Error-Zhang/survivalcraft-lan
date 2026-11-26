using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW.IconMaker;

public class IconMakerPreviewWidget : CanvasWidget
{
	private RenderTarget2D? m_previewRt;

	public IconMakerProject Project { get; set; } = new IconMakerProject();

	public bool IsDirty { get; set; } = true;

	public IconMakerPreviewWidget()
	{
		ClampToBounds = true;
	}

	public override void Update()
	{
		base.Update();
		if (IsDirty)
		{
			m_previewRt = IconMakerRenderer.Compose(Project);
			IsDirty = false;
		}
	}

	public override void Draw(DrawContext dc)
	{
		if (m_previewRt != null)
		{
			float x = ActualSize.X;
			float y = ActualSize.Y;
			float num = MathUtils.Min(x, y);
			Vector2 v = new Vector2((x - num) * 0.5f, (y - num) * 0.5f);
			Vector2 v2 = v + new Vector2(num, 0f);
			Vector2 v3 = v + new Vector2(num);
			Vector2 v4 = v + new Vector2(0f, num);
			Matrix m = GlobalTransform;
			Vector2.Transform(ref v, ref m, out var result);
			Vector2.Transform(ref v2, ref m, out var result2);
			Vector2.Transform(ref v3, ref m, out var result3);
			Vector2.Transform(ref v4, ref m, out var result4);
			dc.PrimitivesRenderer2D.TexturedBatch(m_previewRt, useAlphaTest: false, 0, null, null, null, SamplerState.LinearClamp).QueueQuad(result, result2, result3, result4, 1f, Vector2.Zero, Vector2.UnitX, Vector2.One, Vector2.UnitY, Color.White);
		}
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		IsDrawRequired = true;
		DesiredSize = parentAvailableSize;
	}
}
