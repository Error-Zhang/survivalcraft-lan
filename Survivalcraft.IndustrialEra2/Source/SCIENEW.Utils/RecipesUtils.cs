using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Game;

namespace SCIENEW.Utils;

public static class RecipesUtils
{
	private static Dictionary<string, int[]>? m_blockIndicesByCraftingId;

	private static readonly Dictionary<(string CraftingId, int DataKey), int[]> m_blockValuesByCraftingId = new Dictionary<(string, int), int[]>();

	internal static int CachedBlockValueEntryCount => m_blockValuesByCraftingId.Count;

	internal static bool HasFrozenBlockIndicesByCraftingId => m_blockIndicesByCraftingId != null;

	public static void InvalidateBlockLookupCache()
	{
		m_blockIndicesByCraftingId = null;
		m_blockValuesByCraftingId.Clear();
	}

	internal static void SeedBlockLookupCacheForTests(string craftingId, int? data, int[] values, bool freezeIndices)
	{
		m_blockValuesByCraftingId[(craftingId, data ?? (-1))] = values;
		if (freezeIndices && m_blockIndicesByCraftingId == null)
		{
			m_blockIndicesByCraftingId = new Dictionary<string, int[]>();
		}
	}

	public static int DecodeIEResult(string result)
	{
		bool handled = false;
		int decodedValue = 0;
		ModsManager.HookAction("DecodeResult", (ModLoader modLoader) =>
		{
			decodedValue = modLoader.DecodeResult(result, out handled);
			return handled;
		});
		if (handled)
		{
			return decodedValue;
		}
		if (!string.IsNullOrEmpty(result))
		{
			if (!result.Contains('-'))
			{
				string[] array = result.Split(new char[1] { ':' }, StringSplitOptions.None);
				Block block = BlocksManager.GetBlock(array[0]);
				if (block == null)
				{
					return 0;
				}
				return Terrain.MakeBlockValue(block.BlockIndex, 0, (array.Length == 2) ? int.Parse(array[1], CultureInfo.InvariantCulture) : 0);
			}
			string[] array2 = result.Split("-");
			string text = array2[0];
			string text2 = array2[1];
			switch (text)
			{
			case "I":
				return ItemsBlockManager.GetItemID(text2);
			case "N":
				return NormalBlockManager.GetNormalBlockID(text2);
			case "D":
				return DevicesBlockManager.GetDeviceValue(text2, 0);
			case "H":
				return DamageableItemManager.GetItemID(text2);
			}
		}
		return 0;
	}

	public static int[] ExpandIngredientToBlockValues(string ingredient)
	{
		if (string.IsNullOrEmpty(ingredient))
		{
			return Array.Empty<int>();
		}
		DecodeIEIngredient(ingredient, out string craftingId, out int? data);
		int[] array = FindBlocksByCraftingId(craftingId, data);
		if (array.Length != 0)
		{
			return array;
		}
		return Array.Empty<int>();
	}

	public static int[] ExpandIngredientsToBlockValues(IEnumerable<string> ingredients)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (string ingredient in ingredients)
		{
			int[] array = ExpandIngredientToBlockValues(ingredient);
			foreach (int item in array)
			{
				hashSet.Add(item);
			}
		}
		return hashSet.ToArray();
	}

	public static int[] FindBlocksByCraftingId(string craftingId, int? data)
	{
		int item = data ?? (-1);
		if (m_blockValuesByCraftingId.TryGetValue((craftingId, item), out int[] value))
		{
			return value;
		}
		List<int> list = new List<int>();
		if (!craftingId.Contains('-'))
		{
			int data2 = data ?? 0;
			int[] blockIndicesByCraftingId = GetBlockIndicesByCraftingId(craftingId);
			foreach (int contents in blockIndicesByCraftingId)
			{
				list.Add(Terrain.MakeBlockValue(contents, 0, data2));
			}
		}
		else
		{
			string obj = craftingId.Split('-')[0];
			string text = craftingId.Split('-')[1];
			if (obj == "I")
			{
				TryAddBlockValue(list, ItemsBlockManager.GetItemID(text));
			}
			if (obj == "N")
			{
				TryAddBlockValue(list, NormalBlockManager.GetNormalBlockID(text));
			}
			if (obj == "D")
			{
				TryAddBlockValue(list, DevicesBlockManager.GetDeviceValue(text, 0));
			}
			if (obj == "H")
			{
				TryAddBlockValue(list, DamageableItemManager.GetItemID(text));
			}
		}
		int[] array = list.ToArray();
		m_blockValuesByCraftingId[(craftingId, item)] = array;
		return array;
	}

	private static int[] GetBlockIndicesByCraftingId(string craftingId)
	{
		EnsureBlockIndicesByCraftingIdIndex();
		if (!m_blockIndicesByCraftingId.TryGetValue(craftingId, out int[] value))
		{
			return Array.Empty<int>();
		}
		return value;
	}

	private static void EnsureBlockIndicesByCraftingIdIndex()
	{
		if (m_blockIndicesByCraftingId != null)
		{
			return;
		}
		Dictionary<string, List<int>> dictionary = new Dictionary<string, List<int>>();
		Block[] blocks = BlocksManager.Blocks;
		foreach (Block block in blocks)
		{
			string craftingId = block.CraftingId;
			if (!string.IsNullOrEmpty(craftingId))
			{
				if (!dictionary.TryGetValue(craftingId, out var value))
				{
					value = (dictionary[craftingId] = new List<int>());
				}
				value.Add(block.BlockIndex);
			}
		}
		Dictionary<string, int[]> dictionary2 = new Dictionary<string, int[]>(dictionary.Count);
		foreach (KeyValuePair<string, List<int>> item in dictionary)
		{
			dictionary2[item.Key] = item.Value.ToArray();
		}
		m_blockIndicesByCraftingId = dictionary2;
	}

	private static void TryAddBlockValue(List<int> blocks, int blockValue)
	{
		if (blockValue > 0)
		{
			blocks.Add(blockValue);
		}
	}

	public static string BlockToCratingID(int value)
	{
		Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
		int num = Terrain.ExtractData(value);
		if (!(block is BaseItemBlock))
		{
			if (!(block is BaseDeviceBlock))
			{
				if (!(block is BaseNormalBlock))
				{
					if (block is BaseDamageableItemBlock)
					{
						return "H-" + DamageableItemManager.GetItemCraftingID(value);
					}
					return block.GetCraftingId(value) + ":" + num.ToString(CultureInfo.InvariantCulture);
				}
				return "N-" + NormalBlockManager.GetNormalBlockCraftingID(value);
			}
			return "D-" + DevicesBlockManager.GetDeviceCraftingID(value);
		}
		return "I-" + ItemsBlockManager.GetItemCraftingID(value);
	}

	public static string BlockToIEResult(int value)
	{
		Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
		int num = Terrain.ExtractData(value);
		if (!(block is BaseItemBlock))
		{
			if (!(block is BaseDeviceBlock))
			{
				if (!(block is BaseNormalBlock))
				{
					if (block is BaseDamageableItemBlock)
					{
						return "H-" + DamageableItemManager.GetItemCraftingID(value);
					}
					return block.GetType().Name + ":" + num.ToString(CultureInfo.InvariantCulture);
				}
				return "N-" + NormalBlockManager.GetNormalBlockCraftingID(value);
			}
			return "D-" + DevicesBlockManager.GetDeviceCraftingID(value);
		}
		return "I-" + ItemsBlockManager.GetItemCraftingID(value);
	}

	public static void DecodeIEIngredient(string ingredient, out string craftingId, out int? data)
	{
		bool handled = false;
		string craftingIdHook = string.Empty;
		int? dataHook = null;
		ModsManager.HookAction("DecodeIngredient", (ModLoader modLoader) =>
		{
			modLoader.DecodeIngredient(ingredient, out craftingIdHook, out dataHook, out handled);
			return handled;
		});
		if (handled)
		{
			craftingId = craftingIdHook;
			data = dataHook;
			return;
		}
		string[] array = ingredient.Split(new char[1] { ':' }, StringSplitOptions.None);
		craftingId = array[0];
		data = ((array.Length >= 2) ? new int?(int.Parse(array[1], CultureInfo.InvariantCulture)) : ((int?)null));
	}

	public static bool MatchRecipe(string[] requiredIngredients, string[] actualIngredients)
	{
		bool handled = false;
		bool result = false;
		ModsManager.HookAction("MatchRecipe", (ModLoader modLoader) =>
		{
			result = modLoader.MatchRecipe(requiredIngredients, actualIngredients, out handled);
			return handled;
		});
		if (handled)
		{
			return result;
		}
		if (actualIngredients.Length > 36)
		{
			return false;
		}
		string[] array = new string[36];
		for (int num = 0; num < 2; num++)
		{
			for (int num2 = -6; num2 <= 6; num2++)
			{
				for (int num3 = -6; num3 <= 6; num3++)
				{
					bool flip = num != 0;
					if (!TransformRecipe(array, requiredIngredients, num3, num2, flip))
					{
						continue;
					}
					bool flag = true;
					for (int num4 = 0; num4 < 36; num4++)
					{
						if (num4 == actualIngredients.Length || !CompareIngredients(array[num4], actualIngredients[num4]))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool TransformRecipe(string[] transformedIngredients, string[] ingredients, int shiftX, int shiftY, bool flip)
	{
		Array.Clear(transformedIngredients, 0, transformedIngredients.Length);
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			int num2 = i + shiftY;
			int num3 = num2 * 6;
			if (num2 < 0 || num2 >= 6)
			{
				for (int j = 0; j < 6; j++)
				{
					if (!string.IsNullOrEmpty(ingredients[num + j]))
					{
						return false;
					}
				}
				num += 6;
				continue;
			}
			for (int k = 0; k < 6; k++)
			{
				int num4 = (flip ? (5 - k) : k) + shiftX;
				if (num4 >= 0 && num4 < 6)
				{
					transformedIngredients[num4 + num3] = ingredients[num + k];
				}
				else if (!string.IsNullOrEmpty(ingredients[num + k]))
				{
					return false;
				}
			}
			num += 6;
		}
		return true;
	}

	public static bool CompareIngredients(string requiredIngredient, string actualIngredient, bool throwOnNotSpecified = false)
	{
		if (requiredIngredient == null)
		{
			return actualIngredient == null;
		}
		if (actualIngredient == null)
		{
			return false;
		}
		DecodeIEIngredient(requiredIngredient, out string craftingId, out int? data);
		DecodeIEIngredient(actualIngredient, out string craftingId2, out int? data2);
		bool flag = requiredIngredient.Contains('-');
		bool flag2 = actualIngredient.Contains('-');
		if (!flag && !flag2)
		{
			if (!data2.HasValue)
			{
				if (throwOnNotSpecified)
				{
					throw new InvalidOperationException("Actual ingredient data not specified.");
				}
				return false;
			}
			if (craftingId == craftingId2)
			{
				if (!data.HasValue)
				{
					return true;
				}
				return data.Value == data2.Value;
			}
		}
		else if (flag & flag2)
		{
			return craftingId == craftingId2;
		}
		return false;
	}
}
