using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Engine;
using Game;

namespace SCIENEW;

public static class CreditsCsvLoader
{
	public const string DefaultAssetPath = "Credits/developers.credits";

	public static List<CreditsEntry> LoadDefault()
	{
		return LoadFromContent("Credits/developers.credits");
	}

	public static List<CreditsEntry> LoadFromContent(string absolutePath)
	{
		Stream stream = ContentManager.GetStream(absolutePath);
		if (stream == null)
		{
			Log.Warning("[IECredits] Content not found: " + absolutePath);
			return new List<CreditsEntry>();
		}
		using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, -1, leaveOpen: true);
		return Parse(streamReader.ReadToEnd());
	}

	public static List<CreditsEntry> Parse(string text)
	{
		List<CreditsEntry> list = new List<CreditsEntry>();
		if (string.IsNullOrWhiteSpace(text))
		{
			return list;
		}
		string[] array = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
		bool flag = false;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text2 = array2[i].Trim();
			if (text2.Length == 0 || text2.StartsWith('#'))
			{
				continue;
			}
			if (!flag)
			{
				flag = true;
				if (text2.StartsWith("Name", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
			}
			if (TrySplitCsvRow(text2, out string name, out string role, out string link) && name.Length != 0)
			{
				if (role.Length == 0)
				{
					role = "developer";
				}
				list.Add(new CreditsEntry(name, role, link));
			}
		}
		return list;
	}

	private static bool TrySplitCsvRow(string line, out string name, out string role, out string link)
	{
		name = string.Empty;
		role = string.Empty;
		link = string.Empty;
		int num = line.IndexOf(',');
		if (num < 0)
		{
			name = line.Trim();
			role = "developer";
			return true;
		}
		name = line.Substring(0, num).Trim();
		int num2 = line.IndexOf(',', num + 1);
		if (num2 < 0)
		{
			role = line.Substring(num + 1).Trim();
			return true;
		}
		role = line.Substring(num + 1, num2 - num - 1).Trim();
		link = line.Substring(num2 + 1).Trim();
		return true;
	}
}
