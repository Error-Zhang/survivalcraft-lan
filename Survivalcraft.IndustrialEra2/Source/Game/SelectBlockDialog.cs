using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using SCIENEW;

namespace Game;

public class SelectBlockDialog : Dialog
{
	private readonly Action<int>? m_handler;

	public LabelWidget m_titleWidget;

	public TextBoxWidget m_searchWidget;

	public LabelWidget m_categoryLabel;

	public BevelledButtonWidget m_prevCategoryButton;

	public BevelledButtonWidget m_nextCategoryButton;

	public ListPanelWidget m_blocksList;

	public BevelledButtonWidget m_okButtonWidget;

	public BevelledButtonWidget m_cancelButtonWidget;

	public List<string?> m_categories = new List<string>();

	public int m_categoryIndex;

	private int m_listCategoryIndex = -1;

	private string m_lastSearch = string.Empty;

	public SelectBlockDialog(string title, Action<int>? handler)
	{
		m_handler = handler;
		XElement node = ContentManager.Get<XElement>("Dialogs/SelectBlockDialog");
		LoadContents(this, node);
		m_titleWidget = Children.Find<LabelWidget>("SelectBlockDialog.Title");
		m_searchWidget = Children.Find<TextBoxWidget>("SelectBlockDialog.Search");
		m_categoryLabel = Children.Find<LabelWidget>("SelectBlockDialog.Category");
		m_prevCategoryButton = Children.Find<BevelledButtonWidget>("SelectBlockDialog.PrevCategory");
		m_nextCategoryButton = Children.Find<BevelledButtonWidget>("SelectBlockDialog.NextCategory");
		m_blocksList = Children.Find<ListPanelWidget>("SelectBlockDialog.List");
		m_okButtonWidget = Children.Find<BevelledButtonWidget>("SelectBlockDialog.OkButton");
		m_cancelButtonWidget = Children.Find<BevelledButtonWidget>("SelectBlockDialog.CancelButton");
		m_titleWidget.Text = title;
		m_titleWidget.IsVisible = !string.IsNullOrEmpty(title);
		m_categories.Add(null);
		m_categories.AddRange(BlocksManager.Categories);
		m_blocksList.ItemWidgetFactory = (object item) =>
		{
			int value = (int)item;
			int num = Terrain.ExtractContents(value);
			Block block = BlocksManager.Blocks[num];
			CanvasWidget canvasWidget = new CanvasWidget
			{
				Size = new Vector2(0f, 56f),
				HorizontalAlignment = WidgetAlignment.Stretch
			};
			TechTreeBlockIconWidget widget = new TechTreeBlockIconWidget
			{
				Size = new Vector2(44f),
				HorizontalAlignment = WidgetAlignment.Near,
				VerticalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(4f, 0f),
				Value = value
			};
			canvasWidget.Children.Add(widget);
			canvasWidget.Children.Add(new LabelWidget
			{
				Text = block.GetDisplayName(null, value),
				HorizontalAlignment = WidgetAlignment.Stretch,
				VerticalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(56f, 0f),
				TextAnchor = TextAnchor.VerticalCenter
			});
			return canvasWidget;
		};
		PopulateBlocksList();
		m_lastSearch = m_searchWidget.Text ?? string.Empty;
	}

	public override void Update()
	{
		if (Input.Cancel || m_cancelButtonWidget.IsClicked)
		{
			Dismiss(null);
			return;
		}
		if (m_okButtonWidget.IsClicked && m_blocksList.SelectedItem is int value)
		{
			Dismiss(value);
			return;
		}
		string text = m_searchWidget.Text ?? string.Empty;
		if (m_listCategoryIndex != m_categoryIndex || text != m_lastSearch)
		{
			PopulateBlocksList();
			m_lastSearch = text;
		}
		string value2 = ((m_categories[m_categoryIndex] == null) ? LanguageControl.Get("BlocksManager", "All Blocks") : LanguageControl.Get("BlocksManager", m_categories[m_categoryIndex]));
		m_categoryLabel.Text = $"{value2} ({m_blocksList.Items.Count})";
		m_prevCategoryButton.IsEnabled = m_categoryIndex > 0;
		m_nextCategoryButton.IsEnabled = m_categoryIndex < m_categories.Count - 1;
		if (m_prevCategoryButton.IsClicked)
		{
			m_categoryIndex = MathUtils.Max(m_categoryIndex - 1, 0);
		}
		if (m_nextCategoryButton.IsClicked)
		{
			m_categoryIndex = MathUtils.Min(m_categoryIndex + 1, m_categories.Count - 1);
		}
	}

	public void PopulateBlocksList()
	{
		m_listCategoryIndex = m_categoryIndex;
		string text = m_categories[m_categoryIndex];
		m_blocksList.ScrollPosition = 0f;
		m_blocksList.ClearItems();
		string value = (m_searchWidget.Text ?? string.Empty).Trim();
		List<(Block, int, int)> list = new List<(Block, int, int)>();
		Block[] blocks = BlocksManager.Blocks;
		foreach (Block block in blocks)
		{
			foreach (int creativeValue in block.GetCreativeValues())
			{
				if ((string.IsNullOrEmpty(text) || !(block.GetCategory(creativeValue) != text)) && (string.IsNullOrEmpty(value) || block.GetDisplayName(null, creativeValue).IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0))
				{
					list.Add((block, block.GetDisplayOrder(creativeValue), creativeValue));
				}
			}
		}
		foreach (var item in list.OrderBy(((Block block, int order, int value) o) => o.order))
		{
			m_blocksList.AddItem(item.Item3);
		}
	}

	public void Dismiss(int? result)
	{
		DialogsManager.HideDialog(this);
		if (result.HasValue)
		{
			m_handler?.Invoke(result.Value);
		}
	}
}
