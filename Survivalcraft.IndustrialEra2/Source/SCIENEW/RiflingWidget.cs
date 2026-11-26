using System.Xml.Linq;
using Engine;
using Game;
using RecipaediaEX.Overlay;
using SCIENEW.RecipaediaEX.Overlay;

namespace SCIENEW;

public class RiflingWidget : CanvasWidget, IRecipaediaOverlayHost
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget m_resultSlot;

	public InventorySlotWidget rawSlot;

	public ValueBarWidget m_progress;

	public ComponentRifling m_rifling;

	public ContainerWidget OverlayParent => this;

	public RiflingWidget(IInventory inventory, ComponentRifling furnace)
	{
		m_rifling = furnace;
		XElement node = ContentManager.Get<XElement>("Widgets/RiflingWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		m_resultSlot = Children.Find<InventorySlotWidget>("ResultSlot");
		rawSlot = Children.Find<InventorySlotWidget>("RawSlot");
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
		rawSlot.AssignInventorySlot(m_rifling, 0);
		m_resultSlot.AssignInventorySlot(m_rifling, 1);
		RecipaediaOverlayHostUi.EnsureToggleButton(this, this);
	}

	public RecipaediaCraftingContext? GetCraftingContext()
	{
		if (!m_rifling.IsAddedToProject)
		{
			return null;
		}
		return CraftingOverlayHostHelper.BuildCrafterContext(m_rifling);
	}

	public IRecipePlacementTarget? GetPlacementTarget()
	{
		if (!m_rifling.IsAddedToProject)
		{
			return null;
		}
		return new SCIENEW.RecipaediaEX.Overlay.IndustrialRecipePlacement(m_rifling, new One2OnePlacementTarget(m_rifling, 0, m_rifling.FindRecipe));
	}

	public override void Update()
	{
		m_progress.Value = m_rifling.CraftingProgress;
		if (!m_rifling.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
