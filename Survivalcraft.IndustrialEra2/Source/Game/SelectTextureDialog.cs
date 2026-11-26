using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using SCIENEW.IconMaker;

namespace Game;

public class SelectTextureDialog : Dialog
{
	private readonly Action<string>? m_handler;

	private LabelWidget m_titleWidget;

	private TextBoxWidget m_searchWidget;

	private ListPanelWidget m_list;

	private BevelledButtonWidget m_okButton;

	private BevelledButtonWidget m_cancelButton;

	private string m_lastSearch = string.Empty;

	public SelectTextureDialog(string title, Action<string>? handler)
	{
		m_handler = handler;
		XElement node = ContentManager.Get<XElement>("Dialogs/SelectTextureDialog");
		LoadContents(this, node);
		m_titleWidget = Children.Find<LabelWidget>("SelectTextureDialog.Title");
		m_searchWidget = Children.Find<TextBoxWidget>("SelectTextureDialog.Search");
		m_list = Children.Find<ListPanelWidget>("SelectTextureDialog.List");
		m_okButton = Children.Find<BevelledButtonWidget>("SelectTextureDialog.OkButton");
		m_cancelButton = Children.Find<BevelledButtonWidget>("SelectTextureDialog.CancelButton");
		m_titleWidget.Text = title;
		m_titleWidget.IsVisible = !string.IsNullOrEmpty(title);
		IconMakerAssetCatalog.Refresh();
		m_list.ItemWidgetFactory = (object item) =>
		{
			string text = (string)item;
			return new LabelWidget
			{
				Text = text,
				Size = new Vector2(0f, 44f),
				HorizontalAlignment = WidgetAlignment.Stretch,
				VerticalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(8f, 0f),
				TextAnchor = TextAnchor.VerticalCenter,
				FontScale = 0.75f
			};
		};
		PopulateList();
		m_lastSearch = m_searchWidget.Text ?? string.Empty;
	}

	public override void Update()
	{
		if (Input.Cancel || m_cancelButton.IsClicked)
		{
			DismissResult(null);
			return;
		}
		if (m_okButton.IsClicked && m_list.SelectedItem is string path)
		{
			DismissResult(path);
			return;
		}
		string text = m_searchWidget.Text ?? string.Empty;
		if (text != m_lastSearch)
		{
			PopulateList();
			m_lastSearch = text;
		}
	}

	private void PopulateList()
	{
		string search = (m_searchWidget.Text ?? string.Empty).Trim();
		IEnumerable<string> source = IconMakerAssetCatalog.TexturePaths;
		if (!string.IsNullOrEmpty(search))
		{
			source = source.Where((string p) => p.Contains(search, StringComparison.OrdinalIgnoreCase) || IconMakerAssetCatalog.GetDisplayLabel(p).Contains(search, StringComparison.OrdinalIgnoreCase));
		}
		m_list.ClearItems();
		foreach (string item in source.Take(400))
		{
			m_list.AddItem(item);
		}
	}

	private void DismissResult(string? path)
	{
		DialogsManager.HideDialog(this);
		if (path != null)
		{
			m_handler?.Invoke(path);
		}
	}
}
