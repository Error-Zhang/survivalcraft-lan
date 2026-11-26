using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class GReactorControllerWidget : CanvasWidget
{
	public ComponentGReactorController m_componentChest;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_buttonsGrid;

	public LabelWidget m_stateText;

	public ButtonWidget m_buttonFR1;

	public ButtonWidget m_buttonFR0;

	public ButtonWidget m_buttonCR1;

	public ButtonWidget m_buttonCR0;

	public ButtonWidget m_buttonGR1;

	public ButtonWidget m_buttonGR0;

	public ButtonWidget m_buttonAZ5;

	public GReactorControllerWidget(IInventory inventory, ComponentGReactorController componentChest)
	{
		m_componentChest = componentChest;
		XElement node = ContentManager.Get<XElement>("Widgets/GReactorControllerWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_buttonsGrid = Children.Find<GridPanelWidget>("ButtonsGrid");
		m_stateText = Children.Find<LabelWidget>("StateText");
		m_stateText.Font = IndustrialModLoader.PixelFont;
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
		m_buttonFR1 = new BevelledButtonWidget
		{
			Size = new Vector2(72f, 41.142857f),
			Text = LanguageControl.GetContentWidgets("GReactorControllerWidget", 3)
		};
		m_buttonFR0 = new BevelledButtonWidget
		{
			Size = new Vector2(72f, 41.142857f),
			Text = LanguageControl.GetContentWidgets("GReactorControllerWidget", 4)
		};
		m_buttonCR1 = new BevelledButtonWidget
		{
			Size = new Vector2(72f, 41.142857f),
			Text = LanguageControl.GetContentWidgets("GReactorControllerWidget", 5)
		};
		m_buttonCR0 = new BevelledButtonWidget
		{
			Size = new Vector2(72f, 41.142857f),
			Text = LanguageControl.GetContentWidgets("GReactorControllerWidget", 6)
		};
		m_buttonGR1 = new BevelledButtonWidget
		{
			Size = new Vector2(72f, 41.142857f),
			Text = LanguageControl.GetContentWidgets("GReactorControllerWidget", 7)
		};
		m_buttonGR0 = new BevelledButtonWidget
		{
			Size = new Vector2(72f, 41.142857f),
			Text = LanguageControl.GetContentWidgets("GReactorControllerWidget", 8)
		};
		m_buttonAZ5 = new BevelledButtonWidget
		{
			Size = new Vector2(72f, 41.142857f),
			Text = LanguageControl.GetContentWidgets("GReactorControllerWidget", 9),
			BevelColor = Color.Red,
			CenterColor = Color.Red
		};
		m_buttonsGrid.Children.Add(m_buttonFR1);
		m_buttonsGrid.Children.Add(m_buttonFR0);
		m_buttonsGrid.Children.Add(m_buttonCR1);
		m_buttonsGrid.Children.Add(m_buttonCR0);
		m_buttonsGrid.Children.Add(m_buttonGR1);
		m_buttonsGrid.Children.Add(m_buttonGR0);
		m_buttonsGrid.Children.Add(m_buttonAZ5);
		m_buttonFR1.Children.Find<LabelWidget>().FontScale = 0.8f;
		m_buttonFR0.Children.Find<LabelWidget>().FontScale = 0.8f;
		m_buttonCR1.Children.Find<LabelWidget>().FontScale = 0.8f;
		m_buttonCR0.Children.Find<LabelWidget>().FontScale = 0.8f;
		m_buttonGR1.Children.Find<LabelWidget>().FontScale = 0.8f;
		m_buttonGR0.Children.Find<LabelWidget>().FontScale = 0.8f;
		m_buttonAZ5.Children.Find<LabelWidget>().FontScale = 0.8f;
		m_buttonsGrid.SetWidgetCell(m_buttonFR1, new Point2(0, 0));
		m_buttonsGrid.SetWidgetCell(m_buttonFR0, new Point2(0, 1));
		m_buttonsGrid.SetWidgetCell(m_buttonCR1, new Point2(0, 2));
		m_buttonsGrid.SetWidgetCell(m_buttonCR0, new Point2(0, 3));
		m_buttonsGrid.SetWidgetCell(m_buttonGR1, new Point2(0, 4));
		m_buttonsGrid.SetWidgetCell(m_buttonGR0, new Point2(0, 5));
		m_buttonsGrid.SetWidgetCell(m_buttonAZ5, new Point2(0, 6));
	}

	public override void Update()
	{
		m_stateText.Text = m_componentChest.GetStateText();
		if (m_buttonFR1.IsClicked)
		{
			ComponentGReactorController componentChest = m_componentChest;
			if (componentChest != null && !componentChest.fr1 && componentChest.m_sorterUFuelRodCount > 0)
			{
				IndustrialMachineControl.Change(m_componentChest, IndustrialControl.ControllerAction, 0);
			}
		}
		if (m_buttonFR0.IsClicked)
		{
			ComponentGReactorController componentChest = m_componentChest;
			if (componentChest != null && !componentChest.fr0 && componentChest.m_coreUFuelRodCount > 0)
			{
				IndustrialMachineControl.Change(m_componentChest, IndustrialControl.ControllerAction, 1);
			}
		}
		if (m_buttonAZ5.IsClicked)
		{
			ComponentGReactorController componentChest = m_componentChest;
			if (componentChest != null && !componentChest.cr1 && componentChest.m_sorterControlRodCount > 0)
			{
				IndustrialMachineControl.Change(m_componentChest, IndustrialControl.ControllerAction, 6);
			}
		}
		if (m_buttonCR1.IsClicked)
		{
			ComponentGReactorController componentChest = m_componentChest;
			if (componentChest != null && !componentChest.cr1 && componentChest.m_sorterControlRodCount > 0)
			{
				IndustrialMachineControl.Change(m_componentChest, IndustrialControl.ControllerAction, 2);
			}
		}
		if (m_buttonCR0.IsClicked)
		{
			ComponentGReactorController componentChest = m_componentChest;
			if (componentChest != null && !componentChest.cr0 && componentChest.m_coreControlRodCount > 0)
			{
				IndustrialMachineControl.Change(m_componentChest, IndustrialControl.ControllerAction, 3);
			}
		}
		if (m_buttonGR1.IsClicked)
		{
			ComponentGReactorController componentChest = m_componentChest;
			if (componentChest != null && !componentChest.gr1 && componentChest.m_sorterCarbonRodCount > 0)
			{
				IndustrialMachineControl.Change(m_componentChest, IndustrialControl.ControllerAction, 4);
			}
		}
		if (m_buttonGR0.IsClicked)
		{
			ComponentGReactorController componentChest = m_componentChest;
			if (componentChest != null && !componentChest.gr0 && componentChest.m_coreCarbonRodCount > 0)
			{
				IndustrialMachineControl.Change(m_componentChest, IndustrialControl.ControllerAction, 5);
			}
		}
		m_buttonFR1.IsChecked = m_componentChest.m_sorterUFuelRodCount == 0;
		m_buttonFR0.IsChecked = m_componentChest.m_coreUFuelRodCount == 0;
		m_buttonCR1.IsChecked = m_componentChest.m_sorterControlRodCount == 0;
		m_buttonCR0.IsChecked = m_componentChest.m_coreControlRodCount == 0;
		m_buttonGR1.IsChecked = m_componentChest.m_sorterCarbonRodCount == 0;
		m_buttonGR0.IsChecked = m_componentChest.m_coreCarbonRodCount == 0;
		if (!m_componentChest.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
