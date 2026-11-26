using System;
using System.Xml.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class CentrifugeWidget : CanvasWidget
{
	public GridPanelWidget m_inventoryGrid;

	public InventorySlotWidget m_materialSlot;

	public GridPanelWidget m_resultsGrid;

	public ComponentCentrifuge m_separator;

	public CentrifugeWidget(IInventory inventory, ComponentCentrifuge separator)
	{
		m_separator = separator;
		XElement node = ContentManager.Get<XElement>("Widgets/CentrifugeWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_materialSlot = Children.Find<InventorySlotWidget>("MaterialSlot");
		m_resultsGrid = Children.Find<GridPanelWidget>("ResultsGrid");
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
		num = 1;
		for (int k = 0; k < m_resultsGrid.RowsCount; k++)
		{
			for (int l = 0; l < m_resultsGrid.ColumnsCount; l++)
			{
				InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
				inventorySlotWidget2.AssignInventorySlot(separator, num++);
				m_resultsGrid.Children.Add(inventorySlotWidget2);
				m_resultsGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
			}
		}
		m_materialSlot.AssignInventorySlot(m_separator, m_separator.MaterialSlot);
	}

	public override void Update()
	{
		m_materialSlot.CustomViewMatrix = Matrix.CreateRotationY((float)Time.RealTime * MathF.Pow(m_separator.CraftingProgress, 5f) * 3f) * m_materialSlot.m_blockIconWidget.m_viewMatrix;
		if (!m_separator.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
