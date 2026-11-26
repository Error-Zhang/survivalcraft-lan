using System;
using Engine;
using Game;
using SCIENEW.Tech;

namespace SCIENEW;

public class TechTreeNodeWidget : CanvasWidget
{
	public BevelledButtonWidget BevelledButtonWidget;

	public TechTreeBlockIconWidget BlockIconWidget;

	public TechNode TechNode;

	public Action<TechNode> OnClicked = (TechNode node) =>
	{
	};

	public bool IsChecked
	{
		get
		{
			return BevelledButtonWidget.IsChecked;
		}
		set
		{
			BevelledButtonWidget.IsChecked = value;
		}
	}

	public TechTreeNodeWidget(TechNode techNode)
	{
		Size = new Vector2(72f);
		TechNode = techNode;
		ClampToBounds = true;
		BevelledButtonWidget = new BevelledButtonWidget
		{
			HorizontalAlignment = WidgetAlignment.Center,
			VerticalAlignment = WidgetAlignment.Center,
			IsAutoCheckingEnabled = true
		};
		Children.Add(BevelledButtonWidget);
		BlockIconWidget = new TechTreeBlockIconWidget
		{
			HorizontalAlignment = WidgetAlignment.Center,
			VerticalAlignment = WidgetAlignment.Center,
			Value = techNode.IconBlock,
			Size = new Vector2(48f),
			IsHitTestVisible = false
		};
		Children.Add(BlockIconWidget);
	}

	public override void Update()
	{
		base.Update();
		if (BevelledButtonWidget.IsClicked)
		{
			OnClicked(TechNode);
		}
		BevelledButtonWidget bevelledButtonWidget = BevelledButtonWidget;
		Color bevelColor = (BevelledButtonWidget.CenterColor = GetColor());
		bevelledButtonWidget.BevelColor = bevelColor;
		Color GetColor()
		{
			return TechNode.GetMode() switch
			{
				SCIENEW.Tech.TechNode.Mode.Locked => new Color(181, 172, 154),
				SCIENEW.Tech.TechNode.Mode.Handling => new Color(250, 220, 90),
				SCIENEW.Tech.TechNode.Mode.Unlocked => new Color(100, 172, 200),
				_ => Color.White,
			};
		}
	}
}
