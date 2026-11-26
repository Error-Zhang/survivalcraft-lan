using System.Xml.Linq;
using Engine;
using Game;
using RecipaediaEX.Overlay;
using SCIENEW.RecipaediaEX.Overlay;

namespace SCIENEW;

public class WireComposerWidget : CanvasWidget, IRecipaediaOverlayHost
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget m_resultSlot;

	public InventorySlotWidget m_rawSlot;

	public InventorySlotWidget m_ingredientSlot;

	public ValueBarWidget m_progress;

	public ComponentWireComposer m_filler;

	public ContainerWidget OverlayParent => this;

	public WireComposerWidget(IInventory inventory, ComponentWireComposer furnace)
	{
		m_filler = furnace;
		XElement node = ContentManager.Get<XElement>("Widgets/WireComposerWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		m_resultSlot = Children.Find<InventorySlotWidget>("ResultSlot");
		m_rawSlot = Children.Find<InventorySlotWidget>("RawSlot");
		m_ingredientSlot = Children.Find<InventorySlotWidget>("IngredientSlot");
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
		m_rawSlot.AssignInventorySlot(m_filler, 0);
		m_resultSlot.AssignInventorySlot(m_filler, furnace.ResultSlotIndex);
		m_ingredientSlot.AssignInventorySlot(m_filler, furnace.IngredientSlotIndex);
		RecipaediaOverlayHostUi.EnsureToggleButton(this, this);
	}

	public RecipaediaCraftingContext? GetCraftingContext()
	{
		if (!m_filler.IsAddedToProject)
		{
			return null;
		}
		return CraftingOverlayHostHelper.BuildCrafterContext(m_filler);
	}

	public IRecipePlacementTarget? GetPlacementTarget()
	{
		if (!m_filler.IsAddedToProject)
		{
			return null;
		}
		return new SCIENEW.RecipaediaEX.Overlay.IndustrialRecipePlacement(m_filler, new WireComposerPlacementTarget(m_filler));
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
