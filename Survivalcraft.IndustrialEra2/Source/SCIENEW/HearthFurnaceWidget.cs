using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class HearthFurnaceWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public ComponentHearthFurnace m_componentCraftingTable;

	public LabelWidget m_furnaceLabel;

	public TankWidget m_ironTank;

	public TankWidget m_steelTank;

	public GridPanelWidget m_catGrid;

	public FireWidget m_fire;

	public ValueBarWidget m_progress;

	public LegacyBevelledButtonWidget m_inputPortButton;

	public LegacyBevelledButtonWidget m_outputPortButton;

	public Texture2D m_inputOnTexture;

	public Texture2D m_inputOffTexture;

	public Texture2D m_outputOnTexture;

	public Texture2D m_outputOffTexture;

	public HearthFurnaceWidget(IInventory inventory, ComponentHearthFurnace componentCraftingTable)
	{
		m_componentCraftingTable = componentCraftingTable;
		XElement node = ContentManager.Get<XElement>("Widgets/HearthFurnaceWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_furnaceLabel = Children.Find<LabelWidget>("HearthFurnaceLabel");
		m_ironTank = Children.Find<TankWidget>("IronTank");
		m_steelTank = Children.Find<TankWidget>("SteelTank");
		m_catGrid = Children.Find<GridPanelWidget>("CatGrid");
		m_fire = Children.Find<FireWidget>("Fire");
		m_progress = Children.Find<ValueBarWidget>("Progress");
		m_inputPortButton = Children.Find<LegacyBevelledButtonWidget>("InputPortButton");
		m_outputPortButton = Children.Find<LegacyBevelledButtonWidget>("OutputPortButton");
		m_inputOnTexture = ContentManager.Get<Texture2D>("Textures/Gui/HearthFurnaceInputPortOn");
		m_inputOffTexture = ContentManager.Get<Texture2D>("Textures/Gui/HearthFurnaceInputPortOff");
		m_outputOnTexture = ContentManager.Get<Texture2D>("Textures/Gui/HearthFurnaceCastingPortOn");
		m_outputOffTexture = ContentManager.Get<Texture2D>("Textures/Gui/HearthFurnaceCastingPortOff");
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
		int num2 = 0;
		for (int k = 0; k < m_catGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_catGrid.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(componentCraftingTable, num2++);
				m_catGrid.Children.Add(inventorySlotWidget2);
				m_catGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
			}
		}
		m_ironTank.AssignTank(m_componentCraftingTable, 0);
		m_steelTank.AssignTank(m_componentCraftingTable, 1);
	}

	public override void Update()
	{
		m_outputPortButton.m_rectangleWidget.Texture = (m_componentCraftingTable.IsOutputOpen ? m_outputOnTexture : m_outputOffTexture);
		m_inputPortButton.m_rectangleWidget.Texture = (m_componentCraftingTable.IsInputOpen ? m_inputOnTexture : m_inputOffTexture);
		m_furnaceLabel.Text = LanguageControl.GetContentWidgets("HearthFurnaceWidget", 1) + (m_componentCraftingTable.IsIntegrity ? LanguageControl.GetContentWidgets("HearthFurnaceWidget", 4) : "");
		m_fire.ParticlesPerSecond = (m_componentCraftingTable.IsWorking ? 24 : 0);
		m_inputPortButton.m_rectangleWidget.TextureScale = 0.5f;
		LegacyBevelledButtonWidget inputPortButton = m_inputPortButton;
		LegacyBevelledButtonWidget inputPortButton2 = m_inputPortButton;
		Color color = (m_inputPortButton.BevelColor = Color.White);
		Color color2 = (inputPortButton2.CenterColor = color);
		inputPortButton.Color = color2;
		m_outputPortButton.m_rectangleWidget.TextureScale = 0.5f;
		LegacyBevelledButtonWidget outputPortButton = m_outputPortButton;
		LegacyBevelledButtonWidget outputPortButton2 = m_outputPortButton;
		color = (m_outputPortButton.BevelColor = Color.White);
		color2 = (outputPortButton2.CenterColor = color);
		outputPortButton.Color = color2;
		if (m_inputPortButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentCraftingTable, IndustrialControl.FluidPort, 0);
		}
		if (m_outputPortButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentCraftingTable, IndustrialControl.FluidPort, 1);
		}
		if (!m_componentCraftingTable.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
