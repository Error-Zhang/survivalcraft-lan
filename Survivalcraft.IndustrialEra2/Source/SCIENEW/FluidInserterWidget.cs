using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class FluidInserterWidget : CanvasWidget
{
	public SubsystemTerrain m_subsystemTerrain;

	public ComponentFluidInserter m_component;

	public ComponentBlockEntity m_componentBlockEntity;

	public GridPanelWidget m_inventoryGrid;

	public TankWidget m_fluidTank;

	public ButtonWidget m_inMinusButton;

	public ButtonWidget m_inPlusButton;

	protected readonly ButtonWidget m_outPlusButton;

	protected readonly ButtonWidget m_outMinusButton;

	public FluidInserterWidget(IInventory inventory, ComponentFluidInserter componentDriller)
	{
		m_component = componentDriller;
		m_componentBlockEntity = componentDriller.Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = componentDriller.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		XElement node = ContentManager.Get<XElement>("Widgets/FluidInserterWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_fluidTank = Children.Find<TankWidget>("FluidTank");
		m_inMinusButton = Children.Find<ButtonWidget>("InMinusButton");
		m_inPlusButton = Children.Find<ButtonWidget>("InPlusButton");
		m_outMinusButton = Children.Find<ButtonWidget>("OutMinusButton");
		m_outPlusButton = Children.Find<ButtonWidget>("OutPlusButton");
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
		m_fluidTank.AssignTank(componentDriller, 0);
	}

	public override void Update()
	{
		if (!m_component.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
			return;
		}
		Children.Find<LabelWidget>("InCountText").TextAnchor = TextAnchor.Center;
		Children.Find<LabelWidget>("OutCountText").TextAnchor = TextAnchor.Center;
		Children.Find<LabelWidget>("InCountText").Text = string.Format(LanguageControl.GetContentWidgets("FluidInserterWidget", 3), m_component.m_inIndex.ToString());
		Children.Find<LabelWidget>("OutCountText").Text = string.Format(LanguageControl.GetContentWidgets("FluidInserterWidget", 4), m_component.m_outIndex.ToString());
		if (m_inPlusButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.InputStep, 1);
		}
		if (m_inMinusButton.IsClicked && m_component.m_inIndex > 0)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.InputStep, -1);
		}
		if (m_outPlusButton.IsClicked)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.OutputStep, 1);
		}
		if (m_outMinusButton.IsClicked && m_component.m_outIndex > 0)
		{
			IndustrialMachineControl.Change(m_component, IndustrialControl.OutputStep, -1);
		}
	}
}
