using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class FillerWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget m_resultSlot;

	public InventorySlotWidget rawSlot;

	public InventorySlotWidget m_canSlot;

	public ValueBarWidget m_progress;

	public ComponentFiller m_filler;

	public FillerWidget(IInventory inventory, ComponentFiller furnace)
	{
		m_filler = furnace;
		XElement node = ContentManager.Get<XElement>("Widgets/FillerWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		m_resultSlot = Children.Find<InventorySlotWidget>("ResultSlot");
		rawSlot = Children.Find<InventorySlotWidget>("RawSlot");
		m_canSlot = Children.Find<InventorySlotWidget>("CanSlot");
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
		rawSlot.AssignInventorySlot(m_filler, 0);
		m_resultSlot.AssignInventorySlot(m_filler, furnace.ResultSlotIndex);
		m_canSlot.AssignInventorySlot(m_filler, furnace.CanSlotIndex);
	}

	public override void Update()
	{
		m_progress.Value = m_filler.CraftingProgress;
		if (!m_filler.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
