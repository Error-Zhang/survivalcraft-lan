using System.Xml.Linq;
using Engine;
using Game;
using RecipaediaEX.Overlay;
using SCIENEW.RecipaediaEX.Overlay;

namespace SCIENEW;

public class AdvancedCrusherWidget : CanvasWidget, IRecipaediaOverlayHost
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget m_resultSlot;

	public InventorySlotWidget rawSlot;

	public ValueBarWidget m_progress;

	public ComponentAdvancedCrusher m_advancedCrusher;

	public ContainerWidget OverlayParent => this;

	public AdvancedCrusherWidget(IInventory inventory, ComponentAdvancedCrusher furnace)
	{
		m_advancedCrusher = furnace;
		XElement node = ContentManager.Get<XElement>("Widgets/AdvancedCrusherWidget");
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
		rawSlot.AssignInventorySlot(m_advancedCrusher, 0);
		m_resultSlot.AssignInventorySlot(m_advancedCrusher, 1);
		RecipaediaOverlayHostUi.EnsureToggleButton(this, this);
	}

	public RecipaediaCraftingContext? GetCraftingContext()
	{
		if (!m_advancedCrusher.IsAddedToProject)
		{
			return null;
		}
		return CraftingOverlayHostHelper.BuildCrafterContext(m_advancedCrusher);
	}

	public IRecipePlacementTarget? GetPlacementTarget()
	{
		if (!m_advancedCrusher.IsAddedToProject)
		{
			return null;
		}
		return new SCIENEW.RecipaediaEX.Overlay.IndustrialRecipePlacement(m_advancedCrusher, new One2OnePlacementTarget(m_advancedCrusher, 0, m_advancedCrusher.FindRecipe));
	}

	public override void Update()
	{
		m_progress.Value = m_advancedCrusher.CraftingProgress;
		if (!m_advancedCrusher.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
