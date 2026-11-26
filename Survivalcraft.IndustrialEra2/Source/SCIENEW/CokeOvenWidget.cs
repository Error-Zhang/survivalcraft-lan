using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class CokeOvenWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_metalGrid;

	public ComponentCokeOven m_componentCraftingTable;

	public LabelWidget m_furnaceLabel;

	public InventorySlotWidget m_ashesSlot;

	public InventorySlotWidget m_resultSlot;

	public InventorySlotWidget m_remainSlot;

	public FireWidget m_fire;

	public ValueBarWidget m_progress;

	public CokeOvenWidget(IInventory inventory, ComponentCokeOven componentCraftingTable)
	{
		m_componentCraftingTable = componentCraftingTable;
		XElement node = ContentManager.Get<XElement>("Widgets/CokeOvenWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_metalGrid = Children.Find<GridPanelWidget>("FurnaceGrid");
		m_ashesSlot = Children.Find<InventorySlotWidget>("CatSlot");
		m_furnaceLabel = Children.Find<LabelWidget>("FurnaceLabel");
		m_fire = Children.Find<FireWidget>("Fire");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		m_remainSlot = Children.Find<InventorySlotWidget>("RemainsSlot");
		m_resultSlot = Children.Find<InventorySlotWidget>("ResultSlot");
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
		for (int k = 0; k < m_metalGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_metalGrid.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(componentCraftingTable, num++);
				m_metalGrid.Children.Add(inventorySlotWidget2);
				m_metalGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
			}
		}
		m_resultSlot.AssignInventorySlot(componentCraftingTable, componentCraftingTable.ResultSlotIndex);
		m_remainSlot.AssignInventorySlot(componentCraftingTable, componentCraftingTable.RemainsSlotIndex);
		m_ashesSlot.AssignInventorySlot(componentCraftingTable, componentCraftingTable.AshesSlotIndex);
	}

	public override void Update()
	{
		m_furnaceLabel.Text = LanguageControl.GetContentWidgets("CokeOvenWidget", 1) + " " + (m_componentCraftingTable.IsIntegrity ? LanguageControl.GetContentWidgets("CokeOvenWidget", 4) : "");
		m_fire.ParticlesPerSecond = (m_componentCraftingTable.IsWorking ? 24 : 0);
		m_progress.Value = m_componentCraftingTable.m_craftingProgress;
		if (!m_componentCraftingTable.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
