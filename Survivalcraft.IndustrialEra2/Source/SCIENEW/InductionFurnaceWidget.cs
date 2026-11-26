using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class InductionFurnaceWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget rawSlot;

	public ValueBarWidget m_progress;

	public TankWidget m_tank;

	public ComponentInductionFurnace m_presser;

	public FireWidget m_fire;

	public InductionFurnaceWidget(IInventory inventory, ComponentInductionFurnace furnace)
	{
		m_presser = furnace;
		XElement node = ContentManager.Get<XElement>("Widgets/InductionFurnaceWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		rawSlot = Children.Find<InventorySlotWidget>("RawSlot");
		m_fire = Children.Find<FireWidget>("Fire");
		m_tank = Children.Find<TankWidget>("Tank");
		m_tank.AssignTank(furnace, 0);
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
		m_fire.ParticlesPerSecond = (m_presser.IsWorking ? 24 : 0);
		if (!m_presser.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
