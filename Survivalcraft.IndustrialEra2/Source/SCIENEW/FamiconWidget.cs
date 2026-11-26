using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Game;
using SCIENEW.NES;

namespace SCIENEW;

public class FamiconWidget : CanvasWidget
{
	public ComponentFamicon m_componentFamicon;

	public GridPanelWidget m_inventoryGrid;

	public ListPanelWidget m_gamesList;

	public FamiconWidget(IInventory inventory, ComponentFamicon componentFamicon)
	{
		m_componentFamicon = componentFamicon;
		XElement node = ContentManager.Get<XElement>("Widgets/FamiconWidget");
		LoadContents(this, node);
		m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
		m_gamesList = Children.Find<ListPanelWidget>("GamesList");
		ListPanelWidget gamesList = m_gamesList;
		gamesList.ItemClicked = (Action<object>)Delegate.Combine(gamesList.ItemClicked, (Action<object>)((object item) =>
		{
			if (item is string name && NESManager.TryGet(name, out NESFile nesFile))
			{
				m_componentFamicon.ReloadGame(nesFile);
			}
		}));
		int num = 10;
		for (int num2 = 0; num2 < m_inventoryGrid.RowsCount; num2++)
		{
			for (int num3 = 0; num3 < m_inventoryGrid.ColumnsCount; num3++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				inventorySlotWidget.AssignInventorySlot(inventory, num++);
				m_inventoryGrid.Children.Add(inventorySlotWidget);
				m_inventoryGrid.SetWidgetCell(inventorySlotWidget, new Point2(num3, num2));
			}
		}
		PopulateGamesList();
	}

	public void PopulateGamesList()
	{
		m_gamesList.ScrollPosition = 0f;
		m_gamesList.ClearItems();
		foreach (KeyValuePair<string, NESFile> nesFile in NESManager.NesFiles)
		{
			m_gamesList.AddItem(nesFile.Key);
		}
		m_gamesList.SelectedItem = m_componentFamicon.CurrentGame?.FileName ?? m_gamesList.Items[0];
	}

	public override void Update()
	{
		if (!m_componentFamicon.IsAddedToProject)
		{
			ParentWidget.Children.Remove(this);
		}
	}
}
