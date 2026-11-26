using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using SCIENEW.Tech;

namespace SCIENEW.Dialogs;

public static class TechTreeFileDialog
{
	public static void Show(ContainerWidget parent, string title, Action<TechTreeTemplateItem> onPicked, bool includeSystemPicker = true)
	{
		TechTreeManager.RefreshModTechTreeCatalog();
		List<TechTreeTemplateItem> list = new List<TechTreeTemplateItem>();
		if (includeSystemPicker)
		{
			string label = LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ImportSystemPicker");
			list.Add(new TechTreeTemplateItem(TechTreeTemplateKind.SystemPicker, label));
		}
		string text = LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "ExternalTechTreesPrefix");
		foreach (TechTreeModFile item in TechTreeManager.ModTechTrees.OrderBy((TechTreeModFile x) => x.ModTitle, StringComparer.OrdinalIgnoreCase).ThenBy((TechTreeModFile x) => x.FileName, StringComparer.OrdinalIgnoreCase))
		{
			string label2 = "[" + item.ModTitle + "] " + item.FileName;
			list.Add(new TechTreeTemplateItem(TechTreeTemplateKind.ModEmbedded, label2, item));
		}
		foreach (string item2 in TechTreeManager.EnumerateExternalTechTreeFileNames().OrderBy((string x) => x, StringComparer.OrdinalIgnoreCase))
		{
			list.Add(new TechTreeTemplateItem(TechTreeTemplateKind.ExternalFolder, text + item2, null, item2));
		}
		DialogsManager.ShowDialog(parent, new ListSelectionDialog(title, list, 48f, (object item) => new LabelWidget
		{
			Text = ((TechTreeTemplateItem)item).Label,
			HorizontalAlignment = WidgetAlignment.Center,
			VerticalAlignment = WidgetAlignment.Center
		}, (object item) =>
		{
			if (item is TechTreeTemplateItem obj)
			{
				onPicked(obj);
			}
		}));
	}
}
