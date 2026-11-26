using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class FractionalTowerWidget : CanvasWidget
{
	public ComponentFractionalTower m_componentPump;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_tanksGird;

	public TankWidget m_petroleumTank;

	public LabelWidget m_labelWidget;

	public FractionalWidget m_fractionalWidget;

	public FractionalTowerWidget(IInventory inventory, ComponentFractionalTower componentPump)
	{
		m_componentPump = componentPump;
		XElement node = ContentManager.Get<XElement>("Widgets/FractionalTowerWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_tanksGird = Children.Find<GridPanelWidget>("TanksGrid");
		m_petroleumTank = Children.Find<TankWidget>("PetroleumTank");
		m_labelWidget = Children.Find<LabelWidget>("TowerLabel");
		m_fractionalWidget = Children.Find<FractionalWidget>("Fractional");
		m_petroleumTank.AssignTank(componentPump, 0);
		m_petroleumTank.Size = new Vector2(216f, 72f);
		int num = 1;
		for (int i = 0; i < m_tanksGird.RowsCount; i++)
		{
			for (int j = 0; j < m_tanksGird.ColumnsCount; j++)
			{
				TankWidget tankWidget = new TankWidget
				{
					Size = new Vector2(72f, 144f)
				};
				tankWidget.AssignTank(componentPump, num++);
				m_tanksGird.Children.Add(tankWidget);
				m_tanksGird.SetWidgetCell(tankWidget, new Point2(j, i));
			}
		}
		num = 10;
		for (int k = 0; k < m_inventoryGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_inventoryGrid.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget, new Point2(l, k));
			}
		}
	}

	public override void Update()
	{
		m_labelWidget.Text = LanguageControl.GetContentWidgets("FractionalTowerWidget", 1) + " " + (m_componentPump.IsIntegrity ? LanguageControl.GetContentWidgets("FractionalTowerWidget", 3) : "");
		m_fractionalWidget.ParticlesPerSecond = ((m_componentPump.IsWorking && m_componentPump.GetTankVolume(m_componentPump.PetroleumTankIndex) > 0f) ? 36 : 0);
		if (!m_componentPump.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
