using System.Linq;
using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class ElectricBlastFurnaceWidget : CanvasWidget
{
	public ComponentElectricBlastFurnace_Alloy m_componentPump;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_tanksGird;

	public GridPanelWidget m_catsGird;

	public InventorySlotWidget m_inventorySlot;

	public ValueBarWidget m_progressBar;

	public FireWidget m_fire;

	public LabelWidget m_titleLabel;

	public ElectricBlastFurnaceWidget(IInventory inventory, ComponentElectricBlastFurnace_Alloy componentPump)
	{
		m_componentPump = componentPump;
		XElement node = ContentManager.Get<XElement>("Widgets/ElectricBlastFurnaceWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_tanksGird = Children.Find<GridPanelWidget>("TanksGrid");
		m_catsGird = Children.Find<GridPanelWidget>("CatsGrid");
		m_inventorySlot = Children.Find<InventorySlotWidget>("ResultSlot");
		m_progressBar = Children.Find<ValueBarWidget>("Progress");
		m_fire = Children.Find<FireWidget>("Fire");
		m_titleLabel = Children.Find<LabelWidget>("TitleLabel");
		m_inventorySlot.AssignInventorySlot(componentPump, componentPump.ResultSlotIndex);
		int num = 0;
		for (int i = 0; i < m_tanksGird.RowsCount; i++)
		{
			for (int j = 0; j < m_tanksGird.ColumnsCount; j++)
			{
				StackPanelWidget stackPanelWidget = new StackPanelWidget
				{
					Direction = LayoutDirection.Vertical
				};
				m_tanksGird.Children.Add(stackPanelWidget);
				m_tanksGird.SetWidgetCell(stackPanelWidget, new Point2(j, i));
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(componentPump.m_inductionFurnaces.First((ComponentElectricBlastFurnace_Induction x) => x.m_tankIndex == num), 0);
				stackPanelWidget.Children.Add(inventorySlotWidget);
				TankWidget tankWidget = new TankWidget
				{
					Size = new Vector2(72f, 166f)
				};
				tankWidget.AssignTank(componentPump.m_inductionFurnaces.First((ComponentElectricBlastFurnace_Induction x) => x.m_tankIndex == num), 0);
				stackPanelWidget.Children.Add(tankWidget);
				num++;
			}
		}
		num = 0;
		for (int num2 = 0; num2 < m_catsGird.RowsCount; num2++)
		{
			for (int num3 = 0; num3 < m_catsGird.ColumnsCount; num3++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(componentPump, num++);
				m_catsGird.Children.Add(inventorySlotWidget2);
				m_catsGird.SetWidgetCell(inventorySlotWidget2, new Point2(num3, num2));
			}
		}
		num = 10;
		for (int num4 = 0; num4 < m_inventoryGrid.RowsCount; num4++)
		{
			for (int num5 = 0; num5 < m_inventoryGrid.ColumnsCount; num5++)
			{
				InventorySlotWidget inventorySlotWidget3 = new InventorySlotWidget();
				inventorySlotWidget3.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget3);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget3, new Point2(num5, num4));
			}
		}
	}

	public override void Update()
	{
		m_progressBar.Value = m_componentPump.CraftingProgress;
		m_fire.ParticlesPerSecond = (m_componentPump.IsWorking ? 24 : 0);
		m_titleLabel.Text = LanguageControl.GetContentWidgets("ElectricBlastFurnaceWidget", 1) + (m_componentPump.IsIntegrity ? LanguageControl.GetContentWidgets("ElectricBlastFurnaceWidget", 3) : "");
		if (!m_componentPump.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
