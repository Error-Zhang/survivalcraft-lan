using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class AlloyFurnaceWidget : CanvasWidget
{
	public ComponentAlloyFurnace m_componentPump;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_tanksGird;

	public GridPanelWidget m_catsGird;

	public InventorySlotWidget m_inventorySlot;

	public ValueBarWidget m_progressBar;

	public FireWidget m_fire;

	public AlloyFurnaceWidget(IInventory inventory, ComponentAlloyFurnace componentPump)
	{
		m_componentPump = componentPump;
		XElement node = ContentManager.Get<XElement>("Widgets/AlloyFurnaceWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_tanksGird = Children.Find<GridPanelWidget>("TanksGrid");
		m_catsGird = Children.Find<GridPanelWidget>("CatsGrid");
		m_inventorySlot = Children.Find<InventorySlotWidget>("ResultSlot");
		m_progressBar = Children.Find<ValueBarWidget>("Progress");
		m_fire = Children.Find<FireWidget>("Fire");
		m_inventorySlot.AssignInventorySlot(componentPump, componentPump.ResultSlotIndex);
		int num = 0;
		for (int i = 0; i < m_tanksGird.RowsCount; i++)
		{
			for (int j = 0; j < m_tanksGird.ColumnsCount; j++)
			{
				TankWidget tankWidget = new TankWidget
				{
					Size = new Vector2(72f, 238f)
				};
				tankWidget.AssignTank(componentPump, num++);
				m_tanksGird.Children.Add(tankWidget);
				m_tanksGird.SetWidgetCell(tankWidget, new Point2(j, i));
			}
		}
		num = 0;
		for (int k = 0; k < m_catsGird.RowsCount; k++)
		{
			for (int l = 0; l < m_catsGird.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(componentPump, num++);
				m_catsGird.Children.Add(inventorySlotWidget);
				m_catsGird.SetWidgetCell(inventorySlotWidget, new Point2(l, k));
			}
		}
		num = 10;
		for (int m = 0; m < m_inventoryGrid.RowsCount; m++)
		{
			for (int n = 0; n < m_inventoryGrid.ColumnsCount; n++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget2);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget2, new Point2(n, m));
			}
		}
	}

	public override void Update()
	{
		m_progressBar.Value = m_componentPump.CraftingProgress;
		m_fire.ParticlesPerSecond = ((m_componentPump.CraftingProgress > 0f) ? 24 : 0);
		if (!m_componentPump.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
