using Engine;

namespace Game;

public class IEIconSlotWidget : InventorySlotWidget
{
	public RectangleWidget m_iconWidget;

	public Subtexture Subtexture
	{
		get
		{
			return m_iconWidget.Subtexture;
		}
		set
		{
			m_iconWidget.Subtexture = value;
		}
	}

	public Color FillColor
	{
		get
		{
			return m_iconWidget.FillColor;
		}
		set
		{
			m_iconWidget.FillColor = value;
		}
	}

	public Color OutlineColor
	{
		get
		{
			return m_iconWidget.OutlineColor;
		}
		set
		{
			m_iconWidget.OutlineColor = value;
		}
	}

	public bool TextureLinearFilter
	{
		get
		{
			return m_iconWidget.TextureLinearFilter;
		}
		set
		{
			m_iconWidget.TextureLinearFilter = value;
		}
	}

	public IEIconSlotWidget()
	{
		m_iconWidget = new RectangleWidget
		{
			Size = new Vector2(64f, 64f),
			HorizontalAlignment = WidgetAlignment.Center,
			VerticalAlignment = WidgetAlignment.Center
		};
		Children.Add(m_iconWidget);
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		base.MeasureOverride(parentAvailableSize);
		if (m_inventory != null)
		{
			m_iconWidget.IsVisible = m_inventory.GetSlotCount(m_slotIndex) == 0;
		}
		else
		{
			m_iconWidget.IsVisible = false;
		}
	}
}
