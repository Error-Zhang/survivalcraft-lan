using System;
using System.Xml.Linq;
using Engine;
using Game;
using SCIENEW.Utils;

namespace SCIENEW;

public class AF_BoxGeneratorWidget : CanvasWidget
{
	public ComponentAF_BoxGenerator m_componentChest;

	public GridPanelWidget m_inventoryGrid;

	public GridPanelWidget m_chestGrid;

	public ClickableWidget m_clickableWidget;

	public InventorySlotWidget m_gachaSlot;

	public LabelWidget m_buttonLabel;

	public AF_BoxGeneratorWidget(IInventory inventory, ComponentAF_BoxGenerator componentChest)
	{
		m_componentChest = componentChest;
		XElement node = ContentManager.Get<XElement>("Widgets/AF_BoxGeneratorWidget");
		try
		{
			LoadContents(this, node);
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			throw;
		}
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_chestGrid = Children.Find<GridPanelWidget>("ChestGrid");
		m_clickableWidget = Children.Find<ClickableWidget>("ClickableWidget");
		m_buttonLabel = Children.Find<LabelWidget>("ButtonLabel");
		int num = 0;
		for (int i = 0; i < m_chestGrid.RowsCount; i++)
		{
			for (int j = 0; j < m_chestGrid.ColumnsCount; j++)
			{
				m_gachaSlot = new AF_SlotWidget
				{
					Size = new Vector2(251f, 188.25f)
				};
				m_gachaSlot.AssignInventorySlot(componentChest, num++);
				m_chestGrid.Children.Add(m_gachaSlot);
				m_chestGrid.SetWidgetCell(m_gachaSlot, new Point2(j, i));
			}
		}
		num = 10;
		for (int k = 0; k < m_inventoryGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_inventoryGrid.ColumnsCount; l++)
			{
				CanvasWidget canvasWidget = new CanvasWidget
				{
					Size = new Vector2(62.75f, 62.75f),
					IsHitTestVisible = false
				};
				AF_SlotWidget aF_SlotWidget = new AF_SlotWidget
				{
					Size = new Vector2(60f, 60f)
				};
				aF_SlotWidget.AssignInventorySlot(inventory, num++);
				canvasWidget.Children.Add(aF_SlotWidget);
				m_inventoryGrid.Children.Add(canvasWidget);
				m_inventoryGrid.SetWidgetCell(canvasWidget, new Point2(l, k));
			}
		}
	}

	public override void Update()
	{
		m_gachaSlot.CustomViewMatrix = Matrix.CreateRotationY((float)Time.RealTime * MathF.Pow(m_componentChest.m_gachaAnim, 5f) * 3f) * m_gachaSlot.m_blockIconWidget.m_viewMatrix;
		m_buttonLabel.Text = ((m_componentChest.m_selectedResult != null) ? LanguageUtils.GetText(this, "2") : LanguageUtils.GetText(this, "1"));
		if (!m_componentChest.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
		if (m_clickableWidget.IsClicked)
		{
			IndustrialMachineControl.Change(m_componentChest, IndustrialControl.Dispense, 1);
		}
	}
}
