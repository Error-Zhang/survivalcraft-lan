using System;
using Engine;
using SCIENEW;

namespace Game;

public class ChargeWidget : CanvasWidget
{
	public BevelledRectangleWidget m_rectangleWidget;

	public ChargeContentWidget m_fluidWidget;

	public CustomFontLabelWidget m_volumeLabel;

	public float BevelSize
	{
		get
		{
			return m_rectangleWidget.BevelSize;
		}
		set
		{
			m_rectangleWidget.BevelSize = value;
		}
	}

	public float Value { get; set; }

	public float Capacitance { get; set; }

	public ChargeWidget()
	{
		Size = new Vector2(72f, 72f);
		m_rectangleWidget = new BevelledRectangleWidget
		{
			BevelSize = -2f,
			DirectionalLight = 0.15f,
			CenterColor = Color.Black
		};
		Children.Add(m_rectangleWidget);
		m_fluidWidget = new ChargeContentWidget
		{
			Size = Size - new Vector2(MathF.Abs(BevelSize), MathF.Abs(BevelSize)) * 2f,
			Margin = new Vector2(MathF.Abs(BevelSize), MathF.Abs(BevelSize))
		};
		Children.Add(m_fluidWidget);
		CanvasWidget canvasWidget = new CanvasWidget
		{
			Size = new Vector2(float.MaxValue, 20f),
			HorizontalAlignment = WidgetAlignment.Stretch,
			VerticalAlignment = WidgetAlignment.Near
		};
		Children.Add(canvasWidget);
		m_volumeLabel = new CustomFontLabelWidget
		{
			FontScale = 0.45f,
			TextOffset = new Vector2(-6f, 0f),
			Color = Color.Green,
			Font = IndustrialModLoader.PixelFont,
			HorizontalAlignment = WidgetAlignment.Center,
			VerticalAlignment = WidgetAlignment.Center,
			Text = "0/4L"
		};
		canvasWidget.Children.Add(m_volumeLabel);
	}

	public override void Update()
	{
		base.Update();
		m_fluidWidget.Size = Size - new Vector2(MathF.Abs(BevelSize), MathF.Abs(BevelSize)) * 2f;
		m_fluidWidget.m_relativeVolume = Value / Capacitance;
		m_volumeLabel.Text = Value.ToString("F1") + "/" + Capacitance.ToString("F1") + "JL";
	}
}
