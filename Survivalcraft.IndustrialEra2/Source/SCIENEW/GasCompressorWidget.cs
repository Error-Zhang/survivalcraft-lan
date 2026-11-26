using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class GasCompressorWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget m_resultSlot;

	public InventorySlotWidget rawSlot;

	public ValueBarWidget m_progress;

	public ComponentGasCompressor m_compressor;

	public TankWidget m_tank;

	public GasCompressorWidget(IInventory inventory, ComponentGasCompressor furnace)
	{
		m_compressor = furnace;
		XElement node = ContentManager.Get<XElement>("Widgets/GasCompressorWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		m_resultSlot = Children.Find<InventorySlotWidget>("ResultSlot");
		rawSlot = Children.Find<InventorySlotWidget>("RawSlot");
		m_tank = Children.Find<TankWidget>("Tank");
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
		rawSlot.AssignInventorySlot(m_compressor, 0);
		m_resultSlot.AssignInventorySlot(m_compressor, 1);
		m_tank.AssignTank(m_compressor, 0);
	}

	public override void Update()
	{
		m_progress.Value = m_compressor.CraftingProgress;
		if (!m_compressor.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
