using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class AutoGunWidget : CanvasWidget
{
	public ComponentAutoGun m_gun;

	public ComponentTurretTargeting m_targeting;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_chestGrid;

	public BevelledButtonWidget m_targetingButton;

	public AutoGunWidget(IInventory inventory, ComponentAutoGun gun)
	{
		m_gun = gun;
		m_targeting = gun.Entity.FindComponent<ComponentTurretTargeting>(throwOnError: true);
		XElement node = ContentManager.Get<XElement>("Widgets/AutoGunWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_chestGrid = Children.Find<GridPanelWidget>("ChestGrid");
		m_targetingButton = Children.Find<BevelledButtonWidget>("TargetingButton");
		int num = 0;
		for (int i = 0; i < m_chestGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_chestGrid.ColumnsCount; j++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(gun, num++);
				m_chestGrid.Children.Add(inventorySlotWidget);
				m_chestGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
			}
		}
		num = 10;
		for (int k = 0; k < m_inventoryGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_inventoryGrid.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget2);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
			}
		}
	}

	public override void Update()
	{
		if (m_targetingButton.IsClicked)
		{
			Vector3 position = new Vector3(m_gun.Entity.FindComponent<ComponentBlockEntity>(throwOnError: true).Coordinates) + new Vector3(0.5f);
			ComponentPlayer componentPlayer = m_gun.Project.FindSubsystem<SubsystemPlayers>(throwOnError: true).FindNearestPlayer(position);
			if (componentPlayer != null)
			{
				DialogsManager.ShowDialog(componentPlayer.GuiWidget, new TurretTargetingDialog(m_targeting));
			}
		}
		if (!m_gun.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
