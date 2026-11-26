using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Tech;

namespace SCIENEW.Dialogs;

public static class SelectDependencyDialog
{
	public static void Show(ContainerWidget parent, TechTree tree, TechNode current, string title, Action<string?> onSelected)
	{
		HashSet<string> existing = current.Dependencies.Select((TechNode d) => d.Name).ToHashSet(StringComparer.Ordinal);
		List<string> list = tree.TechNodes.Keys.Where((string k) => k != current.Name && !existing.Contains(k)).OrderBy((string k) => k, StringComparer.OrdinalIgnoreCase).ToList();
		if (list.Count == 0)
		{
			DialogsManager.ShowDialog(parent, (Dialog)new MessageDialog(LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "NoMoreDependencies"), LanguageControl.Get("ContentWidgets", "TechTreeMakerScreen", "NoMoreDependenciesHint"), LanguageControl.Ok, null, (MessageDialogButton _) =>
			{
				onSelected(null);
			}));
			return;
		}
		DialogsManager.ShowDialog(parent, new ListSelectionDialog(title, list, 56f, (object item) =>
		{
			string text = (string)item;
			TechNode node = tree.GetNode(text);
			CanvasWidget canvasWidget = new CanvasWidget
			{
				Size = new Vector2(0f, 56f),
				HorizontalAlignment = WidgetAlignment.Stretch
			};
			if (node != null)
			{
				TechTreeBlockIconWidget widget = new TechTreeBlockIconWidget
				{
					Value = node.IconBlock,
					Size = new Vector2(44f),
					HorizontalAlignment = WidgetAlignment.Near,
					VerticalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(4f, 0f)
				};
				canvasWidget.Children.Add(widget);
			}
			canvasWidget.Children.Add(new LabelWidget
			{
				Text = text,
				HorizontalAlignment = WidgetAlignment.Stretch,
				VerticalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(56f, 0f),
				TextAnchor = TextAnchor.VerticalCenter
			});
			return canvasWidget;
		}, (object item) =>
		{
			onSelected((string)item);
		}));
	}
}
