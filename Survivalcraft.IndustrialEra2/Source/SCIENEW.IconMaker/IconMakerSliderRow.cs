using System;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW.IconMaker;

public sealed class IconMakerSliderRow
{
	public const float RowWidth = 388f;

	public const float RowHeight = 44f;

	public LabelWidget Label;

	public SliderWidget Slider;

	public LabelWidget ValueLabel;

	private readonly Action<float> m_onChanged;

	private float m_lastValue;

	public CanvasWidget Container { get; }

	public IconMakerSliderRow(string label, float min, float max, float granularity, float initial, Action<float> onChanged)
	{
		m_onChanged = onChanged;
		m_lastValue = initial;
		CanvasWidget canvasWidget = new CanvasWidget
		{
			Size = new Vector2(388f, 44f),
			HorizontalAlignment = WidgetAlignment.Stretch
		};
		Label = new LabelWidget
		{
			Text = label,
			Color = new Color(200, 200, 200),
			FontScale = 0.75f,
			Size = new Vector2(132f, 44f),
			TextAnchor = TextAnchor.VerticalCenter,
			VerticalAlignment = WidgetAlignment.Center
		};
		Slider = new SliderWidget
		{
			MinValue = min,
			MaxValue = max,
			Granularity = granularity,
			Value = initial,
			IsLabelVisible = false,
			Size = new Vector2(192f, 40f)
		};
		ValueLabel = new LabelWidget
		{
			Text = Format(initial),
			Color = new Color(180, 180, 180),
			FontScale = 0.7f,
			Size = new Vector2(56f, 44f),
			TextAnchor = TextAnchor.Center
		};
		canvasWidget.Children.Add(Label);
		canvasWidget.Children.Add(Slider);
		canvasWidget.Children.Add(ValueLabel);
		canvasWidget.SetWidgetPosition(Label, Vector2.Zero);
		canvasWidget.SetWidgetPosition(Slider, new Vector2(132f, 2f));
		canvasWidget.SetWidgetPosition(ValueLabel, new Vector2(332f, 0f));
		Container = canvasWidget;
	}

	public void Poll()
	{
		float value = Slider.Value;
		if (!(Math.Abs(value - m_lastValue) < 0.0001f))
		{
			m_lastValue = value;
			ValueLabel.Text = Format(value);
			m_onChanged(value);
		}
	}

	public void Sync(float value)
	{
		m_lastValue = value;
		Slider.Value = value;
		ValueLabel.Text = Format(value);
	}

	private static string Format(float v)
	{
		if (!(Math.Abs(v - MathF.Round(v)) < 0.05f))
		{
			return v.ToString("0.##");
		}
		return MathF.Round(v).ToString("0");
	}
}
