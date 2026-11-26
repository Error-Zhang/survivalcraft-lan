using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class FermenterWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget rawSlot;

	public ValueBarWidget m_progress;

	public GridPanelWidget m_tankWidget;

	public ComponentFermenter m_presser;

	public FermenterWidget(IInventory inventory, ComponentFermenter furnace)
	{
		m_presser = furnace;
		XElement node = ContentManager.Get<XElement>("Widgets/FermenterWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		rawSlot = Children.Find<InventorySlotWidget>("RawSlot");
		m_tankWidget = Children.Find<GridPanelWidget>("TanksGrid");
		TankWidget tankWidget = new TankWidget
		{
			Size = new Vector2(72f, 72f)
		};
		tankWidget.AssignTank(furnace, 0);
		m_tankWidget.Children.Add(tankWidget);
		m_tankWidget.SetWidgetCell(tankWidget, new Point2(0, 0));
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
		rawSlot.AssignInventorySlot(m_presser, 0);
	}

	public override void Update()
	{
		m_progress.Value = m_presser.CraftingProgress;
		if (!m_presser.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
