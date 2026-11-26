using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions;

namespace SCIENEW;

public class RadarContentWidget : Widget
{
	public RadarBatch2D m_batch;

	public bool HighSample { get; set; }

	public Vector2[] Points { get; set; } = new Vector2[8];

	public float Time { get; set; }

	public RadarContentWidget()
	{
		HighSample = !IESettingsManager.Get<bool>("LowQuality");
		m_batch = new RadarBatch2D
		{
			DepthStencilState = DepthStencilState.Default,
			BlendState = BlendState.AlphaBlend,
			RasterizerState = RasterizerState.CullNoneScissor,
			HighSample = HighSample,
			Layer = 2
		};
	}

	public override void Update()
	{
		base.Update();
		m_batch.HighSample = HighSample;
		m_batch.Points = Points;
		m_batch.Time = Time;
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		base.MeasureOverride(parentAvailableSize);
		IsDrawRequired = true;
	}

	public override void Draw(DrawContext dc)
	{
		base.Draw(dc);
		Vector2 zero = Vector2.Zero;
		Vector2 actualSize = ActualSize;
		Matrix globalTransform = GlobalTransform;
		zero = zero.Transform(globalTransform);
		actualSize = actualSize.Transform(globalTransform);
		m_batch.QueueQuad(zero, actualSize, 1f, Vector2.One, Vector2.Zero, Color.White);
		m_batch.TransformTriangles(GlobalTransform, m_batch.TriangleVertices.Count);
		m_batch.Flush();
	}
}
