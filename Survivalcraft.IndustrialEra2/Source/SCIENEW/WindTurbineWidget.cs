using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class WindTurbineWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget m_rotorSlot;

	public ComponentWindTurbine m_component;

	public CustomFontLabelWidget m_stateLabel;

	public CustomFontLabelWidget m_windPowerLabel;

	public string m_noSpaceText;

	public string m_noRotorText;

	public string m_normalText;

	public string m_windPowerText;

	public WindTurbineWidget(IInventory inventory, ComponentWindTurbine component)
	{
		m_component = component;
		XElement node = ContentManager.Get<XElement>("Widgets/WindTurbineWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_rotorSlot = Children.Find<InventorySlotWidget>("RotorSlot");
		m_stateLabel = Children.Find<CustomFontLabelWidget>("StateLabel");
		m_windPowerLabel = Children.Find<CustomFontLabelWidget>("WindPowerLabel");
		int num = 10;
		for (int i = 0; i < m_inventoryGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_inventoryGrid.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
			}
		}
		m_rotorSlot.AssignInventorySlot(m_component, 0);
		m_windPowerLabel.Font = (m_stateLabel.Font = IndustrialModLoader.PixelFont);
		m_noSpaceText = LanguageControl.GetContentWidgets("WindTurbineWidget", 3);
		m_noRotorText = LanguageControl.GetContentWidgets("WindTurbineWidget", 4);
		m_normalText = LanguageControl.GetContentWidgets("WindTurbineWidget", 5);
		m_windPowerText = LanguageControl.GetContentWidgets("WindTurbineWidget", 6);
	}

	public override void Update()
	{
		if (!m_component.m_spaceEnough)
		{
			m_stateLabel.Text = m_noSpaceText;
			m_stateLabel.Color = Color.Red;
		}
		else if (m_component.GetSlotCount(0) <= 0)
		{
			m_stateLabel.Text = m_noRotorText;
			m_stateLabel.Color = Color.Red;
		}
		else
		{
			m_stateLabel.Text = m_normalText;
			m_stateLabel.Color = Color.Green;
		}
		m_windPowerLabel.Text = string.Format(m_windPowerText, m_component.GetWindPower().ToString("F"));
		if (!m_component.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
