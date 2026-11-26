using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using GameEntitySystem;
using RecipaediaEX.UI;
using SCIENEW.Tech;

namespace SCIENEW.Utils;

public static class LanguageUtils
{
	public static string GetText(object owner, params string[] keys)
	{
		IEnumerable<string> first = Array.Empty<string>();
		if (!(owner is Component))
		{
			if (!(owner is TechTree techTree))
			{
				if (!(owner is Subsystem))
				{
					if (!(owner is Screen screen))
					{
						if (!(owner is ContainerWidget))
						{
							if (owner is IRecipaediaCategory)
							{
								first = new _003C_003Ez__ReadOnlyArray<string>(new string[2]
								{
									"Category",
									owner.GetType().Name
								});
							}
						}
						else
						{
							first = new _003C_003Ez__ReadOnlyArray<string>(new string[2]
							{
								"ContentWidgets",
								owner.GetType().Name
							});
						}
					}
					else
					{
						first = new _003C_003Ez__ReadOnlyArray<string>(new string[2]
						{
							"ContentWidgets",
							screen.GetType().Name
						});
					}
				}
				else
				{
					first = new _003C_003Ez__ReadOnlyArray<string>(new string[2]
					{
						"Subsystems",
						owner.GetType().Name
					});
				}
			}
			else
			{
				first = new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "TechTrees", techTree.Name });
			}
		}
		else
		{
			first = new _003C_003Ez__ReadOnlyArray<string>(new string[2]
			{
				"Components",
				owner.GetType().Name
			});
		}
		first = first.Concat(keys);
		return LanguageControl.Get(first.ToArray());
	}

	public static string GetRecipeDescription(string identifier)
	{
		if (identifier.StartsWith('[') && identifier.EndsWith(']'))
		{
			string text = identifier.Substring(1, identifier.Length - 2);
			string text2 = LanguageControl.Get(out var r, "IECraftingRecipes", "Descriptions", text);
			if (r)
			{
				identifier = text2;
			}
		}
		return identifier;
	}

	public static string? GetRecipeMessage(string? message)
	{
		if (string.IsNullOrEmpty(message))
		{
			return message;
		}
		if (message.StartsWith('[') && message.EndsWith(']'))
		{
			string text = message.Substring(1, message.Length - 2);
			string result = LanguageControl.Get(out var r, "IECraftingRecipes", "Messages", text);
			if (r)
			{
				return result;
			}
		}
		return message;
	}
}
