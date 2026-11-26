using Engine;
using Engine.Graphics;
using Engine.Media;
using Game;

namespace SCIENEW;

public class CustomFontLabelWidget : LabelWidget
{
	public new BitmapFont Font
	{
		get
		{
			if (m_font != null)
			{
				return m_font;
			}
			return LabelWidget.BitmapFont;
		}
		set
		{
			if (value != m_font)
			{
				m_font = value;
				m_linesSize = null;
			}
		}
	}

	public Vector2 TextOffset { get; set; }

	public override void Draw(DrawContext dc)
	{
		if (string.IsNullOrEmpty(Text) || Color.A == 0)
		{
			return;
		}
		SamplerState samplerState = (TextureLinearFilter ? SamplerState.LinearClamp : SamplerState.PointClamp);
		FontBatch2D fontBatch2D = dc.PrimitivesRenderer2D.FontBatch(Font, 1, DepthStencilState.None, null, null, samplerState);
		int count = fontBatch2D.TriangleVertices.Count;
		float num = 0f;
		if ((TextAnchor & TextAnchor.VerticalCenter) != 0)
		{
			float num2 = Font.GlyphHeight * FontScale * Font.Scale + (float)(m_lines.Count - 1) * ((Font.GlyphHeight + Font.Spacing.Y) * FontScale * Font.Scale + FontSpacing.Y);
			num = (ActualSize.Y - num2) / 2f;
		}
		else if ((TextAnchor & TextAnchor.Bottom) != 0)
		{
			float num3 = Font.GlyphHeight * FontScale * Font.Scale + (float)(m_lines.Count - 1) * ((Font.GlyphHeight + Font.Spacing.Y) * FontScale * Font.Scale + FontSpacing.Y);
			num = ActualSize.Y - num3;
		}
		TextAnchor anchor = TextAnchor & ~(TextAnchor.VerticalCenter | TextAnchor.Bottom);
		Color color = Color * GlobalColorTransform;
		float num4 = CalculateLineHeight();
		foreach (string line in m_lines)
		{
			float x = 0f;
			if ((TextAnchor & TextAnchor.HorizontalCenter) != 0)
			{
				x = ActualSize.X / 2f;
			}
			else if ((TextAnchor & TextAnchor.Right) != 0)
			{
				x = ActualSize.X;
			}
			bool flag = true;
			Vector2 vector = Vector2.Zero;
			float angle = 0f;
			if (TextOrientation == TextOrientation.Horizontal)
			{
				vector = new Vector2(x, num);
				angle = 0f;
				_ = Display.ScissorRectangle;
				flag = true;
			}
			else if (TextOrientation == TextOrientation.VerticalLeft)
			{
				vector = new Vector2(x, ActualSize.Y + num);
				angle = MathUtils.DegToRad(-90f);
				flag = true;
			}
			vector += TextOffset;
			if (flag)
			{
				if (DropShadow)
				{
					fontBatch2D.QueueText(line, vector + 1f * new Vector2(FontScale), 0f, new Color((byte)0, (byte)0, (byte)0, color.A), anchor, new Vector2(FontScale), FontSpacing, angle);
				}
				fontBatch2D.QueueText(line, vector, 0f, color, anchor, new Vector2(FontScale), FontSpacing, angle);
			}
			num += num4;
		}
		fontBatch2D.TransformTriangles(GlobalTransform, count);
	}
}
