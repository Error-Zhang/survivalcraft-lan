using System.Xml.Linq;
using Engine;
using Game;
using RecipaediaEX.Overlay;
using SCIENEW.RecipaediaEX.Overlay;

namespace SCIENEW;

public class IronFurnaceWidget : CanvasWidget, IRecipaediaOverlayHost
{
	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_furnaceGrid;

	public InventorySlotWidget m_fuelSlot;

	public InventorySlotWidget m_resultSlot;

	public InventorySlotWidget m_remainsSlot;

	public FireWidget m_fire;

	public ValueBarWidget m_progress;

	public ComponentIronFurnace m_componentFurnace;

	public ContainerWidget OverlayParent => this;

	public IronFurnaceWidget(IInventory inventory, ComponentIronFurnace componentFurnace)
	{
		m_componentFurnace = componentFurnace;
		XElement node = ContentManager.Get<XElement>("Widgets/IronFurnaceWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_furnaceGrid = Children.Find<GridPanelWidget>("FurnaceGrid");
		m_fire = Children.Find<FireWidget>("Fire");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		m_resultSlot = Children.Find<InventorySlotWidget>("ResultSlot");
		m_remainsSlot = Children.Find<InventorySlotWidget>("RemainsSlot");
		m_fuelSlot = Children.Find<InventorySlotWidget>("FuelSlot");
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
		num = 0;
		for (int k = 0; k < m_furnaceGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_furnaceGrid.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(componentFurnace, num++);
				m_furnaceGrid.Children.Add(inventorySlotWidget2);
				m_furnaceGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
			}
		}
		m_fuelSlot.AssignInventorySlot(componentFurnace, componentFurnace.FuelSlotIndex);
		m_resultSlot.AssignInventorySlot(componentFurnace, componentFurnace.ResultSlotIndex);
		m_remainsSlot.AssignInventorySlot(componentFurnace, componentFurnace.RemainsSlotIndex);
		RecipaediaOverlayHostUi.EnsureToggleButton(this, this);
	}

	public RecipaediaCraftingContext? GetCraftingContext()
	{
		if (!m_componentFurnace.IsAddedToProject)
		{
			return null;
		}
		return CraftingOverlayHostHelper.BuildCrafterContext(m_componentFurnace);
	}

	public IRecipePlacementTarget? GetPlacementTarget()
	{
		if (!m_componentFurnace.IsAddedToProject)
		{
			return null;
		}
		return new SCIENEW.RecipaediaEX.Overlay.IndustrialRecipePlacement(m_componentFurnace, new FurnacePlacementTarget(m_componentFurnace));
	}

	public override void Update()
	{
		m_fire.ParticlesPerSecond = ((m_componentFurnace.HeatLevel > 0f) ? 24 : 0);
		m_progress.Value = m_componentFurnace.SmeltingProgress;
		if (!m_componentFurnace.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
