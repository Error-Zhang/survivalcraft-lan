using System;
using Engine;
using Game;

namespace SCIENEW;

public class RoundRectangleWidget : Widget
{
	public Vector2 Size { get; set; }

	public float Radius { get; set; }

	public Color FillColor { get; set; }

	public RoundRectangleWidget()
	{
		Size = new Vector2(float.PositiveInfinity);
		Radius = 5f;
		FillColor = Color.Black;
	}

	public override void Draw(DrawContext dc)
	{
		if (FillColor.A != 0)
		{
			Matrix m = GlobalTransform;
			Vector2 v = Vector2.Zero;
			Vector2 v2 = new Vector2(ActualSize.X, 0f);
			Vector2 v3 = ActualSize;
			Vector2 v4 = new Vector2(0f, ActualSize.Y);
			Vector2.Transform(ref v, ref m, out var result);
			Vector2.Transform(ref v2, ref m, out var result2);
			Vector2.Transform(ref v3, ref m, out var result3);
			Vector2.Transform(ref v4, ref m, out var result4);
			float num = MathUtils.Max(MathF.Round(Radius * GlobalTransform.Right.Length()), 1f);
			Color color = FillColor * GlobalColorTransform;
			if (color.A != 0)
			{
				Vector2 vector = result + new Vector2(num, num);
				Vector2 vector2 = result2 + new Vector2(0f - num, num);
				Vector2 vector3 = result3 + new Vector2(0f - num, 0f - num);
				Vector2 vector4 = result4 + new Vector2(num, 0f - num);
				dc.PrimitivesRenderer2D.FlatBatch(1).QueueQuad(vector, vector2, vector3, vector4, 1f, color);
				dc.PrimitivesRenderer2D.FlatBatch(1).QueueDisc(vector, new Vector2(num), 1f, color);
				dc.PrimitivesRenderer2D.FlatBatch(1).QueueDisc(vector2, new Vector2(num), 1f, color);
				dc.PrimitivesRenderer2D.FlatBatch(1).QueueDisc(vector3, new Vector2(num), 1f, color);
				dc.PrimitivesRenderer2D.FlatBatch(1).QueueDisc(vector4, new Vector2(num), 1f, color);
				dc.PrimitivesRenderer2D.FlatBatch(1).QueueQuad(vector + new Vector2(0f - num, 0f), vector, vector4, vector4 + new Vector2(0f - num, 0f), 1f, color);
				dc.PrimitivesRenderer2D.FlatBatch(1).QueueQuad(vector + new Vector2(0f, 0f - num), vector2 + new Vector2(0f, 0f - num), vector2, vector, 1f, color);
				dc.PrimitivesRenderer2D.FlatBatch(1).QueueQuad(vector2, vector2 + new Vector2(num, 0f), vector3 + new Vector2(num, 0f), vector3, 1f, color);
				dc.PrimitivesRenderer2D.FlatBatch(1).QueueQuad(vector4, vector3, vector3 + new Vector2(0f, num), vector4 + new Vector2(0f, num), 1f, color);
			}
		}
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		IsDrawRequired = FillColor.A != 0;
		DesiredSize = Size;
	}
}
