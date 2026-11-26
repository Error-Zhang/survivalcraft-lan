using System;
using Engine;
using Game;
using RecipaediaEX.UI;
using SCIENEW.RecipaediaEX.Items;

namespace SCIENEW.Widgets.Recipes.Slots;

public class FluidRecipeSlotWidget : RecipeSlotWidget
{
	public FluidWidget m_fluidWidget;

	public CustomFontLabelWidget m_volumeLabel;

	public LabelWidget m_fluidNameLabel;

	public float m_volume;

	public bool IsMouseHover => m_interactableWidget.IsMouseHover;

	public FluidRecipeSlotWidget()
	{
		m_bevelRect.CenterColor = Color.Black;
		m_fluidWidget = new FluidWidget
		{
			Size = Size - new Vector2(MathUtils.Abs(m_bevelRect.BevelSize), MathUtils.Abs(m_bevelRect.BevelSize)) * 2f,
			Margin = new Vector2(MathUtils.Abs(m_bevelRect.BevelSize), MathUtils.Abs(m_bevelRect.BevelSize)),
			IsHitTestVisible = false
		};
		Children.Add(m_fluidWidget);
		BevelledRectangleWidget widget = new BevelledRectangleWidget
		{
			Size = new Vector2(float.MaxValue, float.MaxValue),
			BevelSize = m_bevelRect.BevelSize,
			CenterColor = new Color(0, 0, 0, 0),
			IsHitTestVisible = false
		};
		Children.Add(widget);
		Children.Remove(m_highlightedRect);
		Children.Add(m_highlightedRect);
		CanvasWidget canvasWidget = new CanvasWidget
		{
			Size = new Vector2(float.MaxValue, 20f),
			HorizontalAlignment = WidgetAlignment.Stretch,
			VerticalAlignment = WidgetAlignment.Near,
			IsHitTestVisible = false
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
			Text = "0/4L",
			IsHitTestVisible = false
		};
		canvasWidget.Children.Add(m_volumeLabel);
		m_fluidNameLabel = new LabelWidget
		{
			VerticalAlignment = WidgetAlignment.Near,
			HorizontalAlignment = WidgetAlignment.Center,
			FontScale = 0.5f,
			Margin = new Vector2(0f, 5f),
			IsHitTestVisible = false
		};
		Children.Add(m_fluidNameLabel);
	}

	public override void SetResult(IRecipaediaItem result, IRecipaediaRecipeNavigator navigator, params object[] additionalData)
	{
		base.SetResult(result, navigator, additionalData);
		m_volume = (float)additionalData[0];
	}

	public override void ShowContent(IRecipaediaItem content)
	{
		base.ShowContent(content);
		if (content is FluidItem fluidItem)
		{
			m_fluidWidget.Size = Size - new Vector2(MathF.Abs(m_bevelRect.BevelSize), MathF.Abs(m_bevelRect.BevelSize)) * 2f;
			m_fluidWidget.m_relativeVolume = 0.75f;
			m_fluidWidget.m_fluidValue = (int)fluidItem.Value;
			m_volumeLabel.Text = m_volume.ToString("F1") + "L";
		}
	}

	public override void HideContent()
	{
		base.HideContent();
		m_fluidWidget.m_fluidValue = 0;
		m_fluidWidget.m_relativeVolume = 0f;
		m_volumeLabel.Text = "0.0L";
		m_fluidNameLabel.Text = string.Empty;
	}

	public override void Update()
	{
		base.Update();
		bool flag = Index >= 0 && Index < RecipaediaItems.Length && RecipaediaItems[Index] is FluidItem;
		m_volumeLabel.IsVisible = flag && !IsMouseHover;
		if (!flag)
		{
			m_fluidNameLabel.Text = string.Empty;
			return;
		}
		int value = (int)((FluidItem)RecipaediaItems[Index]).Value;
		if (IsMouseHover)
		{
			Fluid fluid = FluidsManager.GetFluid(value);
			m_fluidNameLabel.Text = ((fluid is AirFluid) ? "" : fluid.GetDisplayName(value));
		}
		else
		{
			m_fluidNameLabel.Text = string.Empty;
		}
	}
}
