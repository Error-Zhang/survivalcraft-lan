using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class ReactorTurbineWidget : CanvasWidget
{
	public ComponentReactorTurbine m_componentChest;

	public GridPanelWidget m_inventoryGrid;

	public ValueBarWidget m_progress;

	public LabelWidget m_outputText;

	public ReactorTurbineWidget(IInventory inventory, ComponentReactorTurbine componentChest)
	{
		m_componentChest = componentChest;
		XElement node = ContentManager.Get<XElement>("Widgets/ReactorTurbineWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		m_outputText = Children.Find<LabelWidget>("OutputText");
		m_outputText.Font = IndustrialModLoader.PixelFont;
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
	}

	public override void Update()
	{
		m_progress.Value = 1f - (float)m_componentChest.Output / 64f;
		m_outputText.Text = string.Format(LanguageControl.GetContentWidgets("ReactorTurbineWidget", 3), m_componentChest.OutputVoltage, m_componentChest.OutputPower);
		if (!m_componentChest.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
