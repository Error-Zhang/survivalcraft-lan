using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Engine;
using Engine.Input;
using Game;
using MiniBinaryXml;
using SCIENEW.Utils;
using XmlUtilities;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW.Tech;

public static class TechTreeManager
{
	public const string DefaultName = "Default";

	public const string TechTreeFileExtension = ".tch";

	private const char LegacyUnlockEntrySep = '\u001e';

	private const char LegacyUnlockKvSep = '\u001f';

	private static readonly List<TechTreeModFile> s_modTechTrees = new List<TechTreeModFile>();

	public static IReadOnlyList<TechTreeModFile> ModTechTrees => s_modTechTrees;

	public static string IndustrialDefaultSelectionKey => EncodeModSelection(IndustrialModLoader.ModPackageName, "IndustrialMod.tch");

	public static string ExternalTechTreesDirectory => Storage.CombinePaths(ModsManager.ExternalPath, "TechTrees");

	public static string EncodeModSelection(string packageName, string fileNameInMod)
	{
		return "m:" + packageName + ":" + fileNameInMod;
	}

	public static string EncodeExternalSelection(string externalShortFileName)
	{
		return "e:" + externalShortFileName;
	}

	public static string EncodeDiskSelection(string absolutePath)
	{
		return "d:" + Uri.EscapeDataString(absolutePath);
	}

	public static bool IsIndustrialDefaultSelection(string? selectionKey)
	{
		if (string.IsNullOrEmpty(selectionKey) || !selectionKey.StartsWith("m:", StringComparison.Ordinal))
		{
			return false;
		}
		string text = selectionKey;
		string text2 = text.Substring(2, text.Length - 2);
		int num = text2.IndexOf(':');
		if (num < 0)
		{
			return false;
		}
		string a = text2.Substring(0, num);
		text = text2;
		int num2 = num + 1;
		string a2 = text.Substring(num2, text.Length - num2);
		if (string.Equals(a, IndustrialModLoader.ModPackageName, StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(a2, "IndustrialMod.tch", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public static string SanitizeTechTreeId(string id)
	{
		id = id.Trim();
		char[] array = new char[7] { '\u001e', '\u001f', '|', ';', '\n', '\r', '\t' };
		foreach (char c in array)
		{
			id = id.Replace(c.ToString(), string.Empty);
		}
		if (string.IsNullOrEmpty(id))
		{
			id = "tree";
		}
		return id;
	}

	public static string DeriveTechTreeIdFromName(string name)
	{
		return SanitizeTechTreeId(Storage.SanitizeFileName(string.IsNullOrWhiteSpace(name) ? "Default" : name.Trim(), "_"));
	}

	public static string SerializePerTreeUnlocks(Dictionary<string, string> map)
	{
		if (map.Count == 0)
		{
			return string.Empty;
		}
		return string.Join("\n", (from kv in map.AsValueEnumerable()
			select kv.Key + "\t" + (kv.Value ?? string.Empty)).ToArray<FromDictionary<string, string>, KeyValuePair<string, string>, string>());
	}

	public static Dictionary<string, string> DeserializePerTreeUnlocks(string data)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		if (string.IsNullOrEmpty(data))
		{
			return dictionary;
		}
		if (data.IndexOf('\u001e') >= 0 || data.IndexOf('\u001f') >= 0)
		{
			return DeserializePerTreeUnlocksLegacy(data);
		}
		string[] array = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			int num = text.IndexOf('\t');
			if (num > 0)
			{
				string text2 = text.Substring(0, num);
				string text3 = text;
				int num2 = num + 1;
				string value = text3.Substring(num2, text3.Length - num2);
				if (!string.IsNullOrEmpty(text2))
				{
					dictionary[text2] = value;
				}
			}
		}
		return dictionary;
	}

	private static Dictionary<string, string> DeserializePerTreeUnlocksLegacy(string data)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		string[] array = data.Split('\u001e', StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			int num = text.IndexOf('\u001f');
			if (num > 0)
			{
				string text2 = text.Substring(0, num);
				string text3 = text;
				int num2 = num + 1;
				string value = text3.Substring(num2, text3.Length - num2);
				if (!string.IsNullOrEmpty(text2))
				{
					dictionary[text2] = value;
				}
			}
		}
		return dictionary;
	}

	public static TechTree? TryLoadTechTreeBySelectionKey(string selectionKey, out string? error)
	{
		error = null;
		if (string.IsNullOrWhiteSpace(selectionKey))
		{
			error = "empty key";
			return null;
		}
		try
		{
			if (selectionKey.StartsWith("m:", StringComparison.Ordinal))
			{
				string text = selectionKey;
				string text2 = text.Substring(2, text.Length - 2);
				int num = text2.IndexOf(':');
				if (num < 0)
				{
					error = "bad mod key";
					return null;
				}
				string pkg = text2.Substring(0, num);
				text = text2;
				int num2 = num + 1;
				string fname = text.Substring(num2, text.Length - num2);
				TechTreeModFile techTreeModFile = ModTechTrees.FirstOrDefault((TechTreeModFile t) => string.Equals(t.PackageName, pkg, StringComparison.Ordinal) && string.Equals(t.FileName, fname, StringComparison.OrdinalIgnoreCase));
				if (techTreeModFile == null)
				{
					RefreshModTechTreeCatalog();
					techTreeModFile = ModTechTrees.FirstOrDefault((TechTreeModFile t) => string.Equals(t.PackageName, pkg, StringComparison.Ordinal) && string.Equals(t.FileName, fname, StringComparison.OrdinalIgnoreCase));
				}
				if (techTreeModFile == null)
				{
					error = "mod tech tree not found";
					return null;
				}
				return DecodeTechTree(techTreeModFile.Root);
			}
			if (selectionKey.StartsWith("e:", StringComparison.Ordinal))
			{
				string text = selectionKey;
				return LoadTechTreeFromExternalFile(text.Substring(2, text.Length - 2));
			}
			if (selectionKey.StartsWith("d:", StringComparison.Ordinal))
			{
				string text = selectionKey;
				using FileStream stream = new FileStream(Uri.UnescapeDataString(text.Substring(2, text.Length - 2)), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				return LoadTechTreeFromStream(stream);
			}
		}
		catch (Exception ex)
		{
			error = ex.Message;
			return null;
		}
		error = "unknown selection prefix";
		return null;
	}

	public static void Init()
	{
		RefreshModTechTreeCatalog();
		ScreensManager.m_screens["TechTree"] = new TechTreeScreen();
	}

	public static IEnumerable<KeyValuePair<string, object>> InitKeySettings()
	{
		yield return new KeyValuePair<string, object>("TechTree", Key.Z);
		yield return new KeyValuePair<string, object>("Recipaedia", Key.X);
	}

	public static void RefreshModTechTreeCatalog()
	{
		s_modTechTrees.Clear();
		foreach (ModEntity modEntity in ModsManager.ModList)
		{
			modEntity.GetFiles(".tch", (string _, Stream stream) =>
			{
				try
				{
					XElement root = new MiniBinaryXmlReader().ReadAsXml(stream);
					string modTitle = (string.IsNullOrWhiteSpace(modEntity.modInfo.Name) ? modEntity.modInfo.PackageName : modEntity.modInfo.Name);
					s_modTechTrees.Add(new TechTreeModFile(modEntity.modInfo.PackageName, modTitle, _.ToString(), root));
				}
				catch (Exception ex)
				{
					Log.Error("[TechTreeManager] Skip .tch in mod " + modEntity.modInfo.PackageName + ": " + ex.Message);
				}
			});
		}
	}

	public static XElement? GetDefaultWorldTechTreeXml()
	{
		if (s_modTechTrees.Count == 0)
		{
			RefreshModTechTreeCatalog();
		}
		XElement xElement = s_modTechTrees.FirstOrDefault((TechTreeModFile t) => string.Equals(t.PackageName, IndustrialModLoader.ModPackageName, StringComparison.Ordinal) && string.Equals(t.FileName, "IndustrialMod.tch", StringComparison.OrdinalIgnoreCase))?.Root;
		if (xElement == null)
		{
			TechTreeModFile? techTreeModFile = s_modTechTrees.FirstOrDefault();
			if (techTreeModFile == null)
			{
				return null;
			}
			xElement = techTreeModFile.Root;
		}
		return xElement;
	}

	public static void EnsureExternalTechTreesDirectory()
	{
		Storage.CreateDirectory(ExternalTechTreesDirectory);
	}

	public static IEnumerable<string> EnumerateExternalTechTreeFileNames()
	{
		EnsureExternalTechTreesDirectory();
		foreach (string item in Storage.ListFileNames(ExternalTechTreesDirectory))
		{
			if (Storage.GetExtension(item).Equals(".tch", StringComparison.OrdinalIgnoreCase))
			{
				yield return item;
			}
		}
	}

	private static string ExternalTechTreeLogicalPath(string fileName)
	{
		if (!fileName.EndsWith(".tch", StringComparison.OrdinalIgnoreCase))
		{
			fileName += ".tch";
		}
		return Storage.CombinePaths(ExternalTechTreesDirectory, fileName);
	}

	public static TechTree LoadTechTreeFromExternalFile(string fileName)
	{
		EnsureExternalTechTreesDirectory();
		string text = ExternalTechTreeLogicalPath(fileName);
		if (!Storage.FileExists(text))
		{
			throw new FileNotFoundException("Tech tree file not found: " + text);
		}
		using Stream stream = Storage.OpenFile(text, OpenFileMode.Read);
		return LoadTechTreeFromStream(stream);
	}

	public static TechTree LoadTechTreeFromStream(Stream stream)
	{
		return DecodeTechTree(new MiniBinaryXmlReader().ReadAsXml(stream));
	}

	public static void SaveTechTreeToExternalFile(TechTree tree, string fileName)
	{
		EnsureExternalTechTreesDirectory();
		using Stream stream = Storage.OpenFile(ExternalTechTreeLogicalPath(fileName), OpenFileMode.Create);
		new MiniBinaryXmlWriter().Write(EncodeTechTree(tree), stream);
	}

	public static string GetExternalTechTreeAbsoluteUserPath(string fileName)
	{
		return Storage.GetSystemPath(ExternalTechTreeLogicalPath(fileName));
	}

	public static bool TryRevealExternalTechTreeFile(string fileName)
	{
		try
		{
			string path = ExternalTechTreeLogicalPath(fileName);
			if (!Storage.FileExists(path))
			{
				return false;
			}
			Storage.OpenFileWithExternalApplication(path);
			return true;
		}
		catch (Exception ex)
		{
			Log.Warning("[TechTreeManager] TryRevealExternalTechTreeFile: " + ex.Message);
			return false;
		}
	}

	public static TechTree DecodeTechTree(XElement? xTechTree)
	{
		if (xTechTree == null)
		{
			return new TechTree("Default");
		}
		string attributeValue = XmlUtils.GetAttributeValue(xTechTree, "Name", "Default");
		string attributeValue2 = XmlUtils.GetAttributeValue(xTechTree, "Id", string.Empty);
		TechTree techTree = new TechTree(attributeValue, string.IsNullOrWhiteSpace(attributeValue2) ? null : attributeValue2);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (XElement item in xTechTree.Elements())
		{
			if (item.Name == "Node")
			{
				string attributeValue3 = XmlUtils.GetAttributeValue<string>(item, "Name");
				TechNode orCreateNode = techTree.GetOrCreateNode(attributeValue3);
				orCreateNode.IconBlock = RecipesUtils.DecodeIEResult(XmlUtils.GetAttributeValue<string>(item, "Icon"));
				orCreateNode.Stage = XmlUtils.GetAttributeValue<int>(item, "Stage");
				orCreateNode.DisplayName = XmlUtils.GetAttributeValue(item, "DisplayName", LanguageUtils.GetText(techTree, "DisplayNames", attributeValue3));
				orCreateNode.Description = XmlUtils.GetAttributeValue(item, "Description", LanguageUtils.GetText(techTree, "Descriptions", attributeValue3));
				orCreateNode.LayoutOffset = XmlUtils.GetAttributeValue(item, "LayoutOffset", Vector2.Zero);
				XAttribute xAttribute = item.Attribute("StageOrder");
				if (xAttribute != null && int.TryParse(xAttribute.Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
				{
					orCreateNode.StageOrder = result;
				}
				else
				{
					int stage = orCreateNode.Stage;
					dictionary.TryGetValue(stage, out var value);
					orCreateNode.StageOrder = value;
					dictionary[stage] = value + 1;
				}
			}
			if (!(item.Name == "Dependencies"))
			{
				continue;
			}
			string attributeValue4 = XmlUtils.GetAttributeValue<string>(item, "Name");
			string value2 = item.Value;
			if (!string.IsNullOrEmpty(value2))
			{
				string[] array = value2.Split("\n", StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = string.Join("", array[i].Split((string[]?)null, StringSplitOptions.RemoveEmptyEntries));
				}
				array = (from x in array.AsValueEnumerable()
					where !string.IsNullOrEmpty(x)
					select x).ToArray();
				techTree.AddDependency(attributeValue4, array);
			}
		}
		return techTree;
	}

	public static XElement EncodeTechTree(TechTree tree)
	{
		XElement xElement = new XElement("TechTree");
		xElement.SetAttributeValue("Name", string.IsNullOrWhiteSpace(tree.Name) ? "Default" : tree.Name);
		xElement.SetAttributeValue("Id", string.IsNullOrWhiteSpace(tree.Id) ? DeriveTechTreeIdFromName(tree.Name) : tree.Id);
		ValueEnumerator<OrderBy<FromEnumerable<TechNode>, TechNode, string>, TechNode> enumerator = ValueEnumerableExtensions.GetEnumerator<OrderBy<FromEnumerable<TechNode>, TechNode, string>, TechNode>(in ILSpyHelper_AsRefReadOnly((from n in tree.TechNodes.Values.AsValueEnumerable()
			orderby n.Stage
			select n).ThenBy<FromEnumerable<TechNode>, TechNode, int, int>((Func<TechNode, int>)((TechNode n) => n.StageOrder)).ThenBy<FromEnumerable<TechNode>, TechNode, int, string>((Func<TechNode, string>)((TechNode n) => n.Name), (IComparer<string>?)StringComparer.Ordinal)));
		try
		{
			while (enumerator.MoveNext())
			{
				TechNode current = enumerator.Current;
				XElement xElement2 = new XElement("Node");
				xElement2.SetAttributeValue("Name", current.Name);
				xElement2.SetAttributeValue("Icon", RecipesUtils.BlockToIEResult(current.IconBlock));
				xElement2.SetAttributeValue("Stage", current.Stage.ToString(CultureInfo.InvariantCulture));
				xElement2.SetAttributeValue("StageOrder", current.StageOrder.ToString(CultureInfo.InvariantCulture));
				string text = LanguageUtils.GetText(tree, "DisplayNames", current.Name);
				string text2 = LanguageUtils.GetText(tree, "Descriptions", current.Name);
				string text3 = current.DisplayName ?? string.Empty;
				string text4 = current.Description ?? string.Empty;
				if (!string.Equals(text3, text, StringComparison.Ordinal))
				{
					xElement2.SetAttributeValue("DisplayName", text3);
				}
				if (!string.Equals(text4, text2, StringComparison.Ordinal))
				{
					xElement2.SetAttributeValue("Description", text4);
				}
				if (current.LayoutOffset != Vector2.Zero)
				{
					xElement2.SetAttributeValue("LayoutOffset", string.Format(CultureInfo.InvariantCulture, "{0}, {1}", current.LayoutOffset.X, current.LayoutOffset.Y));
				}
				xElement.Add(xElement2);
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		ValueEnumerator<OrderBy<FromEnumerable<TechNode>, TechNode, string>, TechNode> enumerator2 = ValueEnumerableExtensions.GetEnumerator<OrderBy<FromEnumerable<TechNode>, TechNode, string>, TechNode>(in ILSpyHelper_AsRefReadOnly(tree.TechNodes.Values.AsValueEnumerable().OrderBy((TechNode n) => n.Name, StringComparer.Ordinal)));
		try
		{
			while (enumerator2.MoveNext())
			{
				TechNode current2 = enumerator2.Current;
				if (current2.Dependencies.Count != 0)
				{
					string text5 = "\n" + string.Join("\n", (from d in current2.Dependencies.AsValueEnumerable()
						select d.Name).ToArray<ListSelect<TechNode, string>, string>()) + "\n";
					xElement.Add(new XElement("Dependencies", new XAttribute("Name", current2.Name), text5));
				}
			}
			return xElement;
		}
		finally
		{
			enumerator2.Dispose();
		}
		static ref readonly T ILSpyHelper_AsRefReadOnly<T>(in T temp) where T : allows ref struct
		{
			//ILSpy generated this function to help ensure overload resolution can pick the overload using 'in'
			return ref temp;
		}
	}

	public static bool IsOpenTechTree()
	{
		object keyboardMapping = SettingsManager.GetKeyboardMapping("TechTree");
		if (!(keyboardMapping is Key key))
		{
			if (keyboardMapping is MouseButton mouseButton)
			{
				return Mouse.IsMouseButtonDown(mouseButton);
			}
			return false;
		}
		return key != Key.Null && Keyboard.IsKeyDownOnce(key);
	}

	public static bool IsOpenRecipaedia()
	{
		object keyboardMapping = SettingsManager.GetKeyboardMapping("Recipaedia");
		if (!(keyboardMapping is Key key))
		{
			if (keyboardMapping is MouseButton mouseButton)
			{
				return Mouse.IsMouseButtonDown(mouseButton);
			}
			return false;
		}
		return key != Key.Null && Keyboard.IsKeyDownOnce(key);
	}

	public static void StartChooseTchFromSystem(ContainerWidget host, string filterLabel, string dialogTitle, Action<TechTree, string?> onLoaded)
	{
		Task.Run(async () =>
		{
			_ = 1;
			try
			{
				KeyValuePair<string, string[]>[] filters = new KeyValuePair<string, string[]>[1]
				{
					new KeyValuePair<string, string[]>(filterLabel, new string[1] { "*.tch" })
				};
				Stream item = (await Storage.ChooseFile(dialogTitle, filters)).Item1;
				if (item == null)
				{
					return;
				}
				try
				{
					string abs = ((item is FileStream fileStream) ? fileStream.Name : null);
					await using (item)
					{
						TechTree tree = LoadTechTreeFromStream(item);
						Dispatcher.Dispatch(() =>
						{
							onLoaded(tree, abs);
						});
					}
				}
				catch (Exception ex)
				{
					Exception ex2 = ex;
					Exception ex3 = ex2;
					Dispatcher.Dispatch(() =>
					{
						DialogsManager.ShowDialog(host, (Dialog)new MessageDialog(LanguageControl.Error, ex3.Message, LanguageControl.Ok, null, (MessageDialogButton _) =>
						{
						}));
					});
				}
			}
			catch (Exception ex4)
			{
				Exception ex2 = ex4;
				Exception ex5 = ex2;
				Dispatcher.Dispatch(() =>
				{
					DialogsManager.ShowDialog(host, (Dialog)new MessageDialog(LanguageControl.Error, ex5.Message, LanguageControl.Ok, null, (MessageDialogButton _) =>
					{
					}));
				});
			}
		});
	}
}
