using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Graphics;
using Game;

namespace SCIENEW.IconMaker;

public static class IconMakerAssetCatalog
{
	private static List<string>? s_texturePaths;

	public static IReadOnlyList<string> TexturePaths
	{
		get
		{
			if (s_texturePaths == null)
			{
				Refresh();
			}
			return s_texturePaths;
		}
	}

	public static void Refresh()
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (ContentInfo item in ContentManager.List())
		{
			switch (item.ContentSuffix)
			{
			case ".png":
			case ".webp":
			case ".jpg":
			case ".jpeg":
				if (!string.IsNullOrEmpty(item.ContentPath))
				{
					hashSet.Add(item.ContentPath);
				}
				break;
			}
		}
		s_texturePaths = hashSet.OrderBy((string p) => p, StringComparer.OrdinalIgnoreCase).ToList();
	}

	public static Texture2D LoadTexture(string contentPath)
	{
		try
		{
			return ContentManager.Get<Texture2D>(contentPath);
		}
		catch
		{
			return ContentManager.Get<Texture2D>("Textures/White");
		}
	}

	public static string GetDisplayLabel(string contentPath)
	{
		int num = contentPath.LastIndexOf('/');
		if (num < 0)
		{
			return contentPath;
		}
		int num2 = num + 1;
		return contentPath.Substring(num2, contentPath.Length - num2);
	}
}
