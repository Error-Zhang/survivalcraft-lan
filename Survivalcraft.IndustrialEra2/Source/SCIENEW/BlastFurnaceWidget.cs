using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class BlastFurnaceWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_metalGrid;

	public ComponentBlastFurnace m_componentCraftingTable;

	public LabelWidget m_furnaceLabel;

	public TankWidget m_metalTank;

	public GridPanelWidget m_catGrid;

	public FireWidget m_fire;

	public ValueBarWidget m_progress;

	public LegacyBevelledButtonWidget m_portButton;

	public Texture2D m_onTexture;

	public Texture2D m_offTexture;

	public BlastFurnaceWidget(IInventory inventory, ComponentBlastFurnace componentCraftingTable)
	{
		m_componentCraftingTable = componentCraftingTable;
		XElement node = ContentManager.Get<XElement>("Widgets/BlastFurnaceWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_metalGrid = Children.Find<GridPanelWidget>("MetalGrid");
		m_furnaceLabel = Children.Find<LabelWidget>("BlastFurnaceLabel");
		m_metalTank = Children.Find<TankWidget>("MetalTank");
		m_catGrid = Children.Find<GridPanelWidget>("CatGrid");
		m_fire = Children.Find<FireWidget>("Fire");
		m_portButton = Children.Find<LegacyBevelledButtonWidget>("PortButton");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		m_onTexture = ContentManager.Get<Texture2D>("Textures/Gui/CastingPortOn");
		m_offTexture = ContentManager.Get<Texture2D>("Textures/Gui/CastingPortOff");
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
		int metalSlotIndex = m_componentCraftingTable.MetalSlotIndex;
		for (int k = 0; k < m_metalGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_metalGrid.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(componentCraftingTable, metalSlotIndex++);
				m_metalGrid.Children.Add(inventorySlotWidget2);
				m_metalGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
			}
		}
		metalSlotIndex = m_componentCraftingTable.CatSlotIndex;
		for (int m = 0; m < m_catGrid.RowsCount; m++)
		{
			for (int n = 0; n < m_catGrid.ColumnsCount; n++)
			{
				InventorySlotWidget inventorySlotWidget3 = new InventorySlotWidget();
				inventorySlotWidget3.AssignInventorySlot(componentCraftingTable, metalSlotIndex++);
				m_catGrid.Children.Add(inventorySlotWidget3);
				m_catGrid.SetWidgetCell(inventorySlotWidget3, new Point2(n, m));
			}
		}
		m_metalTank.AssignTank(m_componentCraftingTable, 0);
	}

	public override void Update()
	{
		m_furnaceLabel.Text = LanguageControl.GetContentWidgets("BlastFurnaceWidget", 3) + (m_componentCraftingTable.IsIntegrity ? (" " + LanguageControl.GetContentWidgets("BlastFurnaceWidget", 5) + " " + m_componentCraftingTable.m_ports.Count + " H" + m_componentCraftingTable.m_height) : "");
		m_fire.ParticlesPerSecond = (m_componentCraftingTable.IsWorking ? 24 : 0);
		m_progress.Value = m_componentCraftingTable.m_progress;
		LegacyBevelledButtonWidget portButton = m_portButton;
		LegacyBevelledButtonWidget portButton2 = m_portButton;
		Color color = (m_portButton.BevelColor = Color.White);
		Color color2 = (portButton2.CenterColor = color);
		portButton.Color = color2;
		m_portButton.m_rectangleWidget.Texture = (m_componentCraftingTable.IsPortOpen ? m_onTexture : m_offTexture);
		m_portButton.m_rectangleWidget.TextureScale = 0.5f;
		if (m_portButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentCraftingTable, IndustrialControl.FluidPort, 0);
		}
		if (!m_componentCraftingTable.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
