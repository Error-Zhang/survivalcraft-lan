using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Game;

namespace SCIENEW;

public static class OresDataBase
{
	private static readonly Dictionary<Materials, List<(Materials Ore, float Probability)>> companionOresMap;

	private static readonly Dictionary<Materials, float> mineralProbabilities;

	private static readonly Dictionary<Materials, float> polymetallicNoduleDropTable;

	static OresDataBase()
	{
		polymetallicNoduleDropTable = new Dictionary<Materials, float>
		{
			{
				Materials.Copper,
				0.38f
			},
			{
				Materials.Iron,
				0.32f
			},
			{
				Materials.Nickel,
				0.15f
			},
			{
				Materials.Zinc,
				0.08f
			},
			{
				Materials.Platinum,
				0.04f
			},
			{
				Materials.Titanium,
				0.02f
			},
			{
				Materials.Uranium,
				0.015f
			},
			{
				Materials.Silver,
				0.008f
			},
			{
				Materials.Gold,
				0.007f
			}
		};
		companionOresMap = new Dictionary<Materials, List<(Materials, float)>>();
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary = companionOresMap;
		int num = 2;
		List<(Materials, float)> list = new List<(Materials, float)>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<(Materials, float)> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = (Materials.Silver, 0.3f);
		num2++;
		span[num2] = (Materials.Mercury, 0.1f);
		dictionary.Add(Materials.Gold, list);
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary2 = companionOresMap;
		num2 = 2;
		List<(Materials, float)> list2 = new List<(Materials, float)>(num2);
		CollectionsMarshal.SetCount(list2, num2);
		Span<(Materials, float)> span2 = CollectionsMarshal.AsSpan(list2);
		num = 0;
		span2[num] = (Materials.Gold, 0.1f);
		num++;
		span2[num] = (Materials.Copper, 0.3f);
		dictionary2.Add(Materials.Silver, list2);
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary3 = companionOresMap;
		num = 2;
		List<(Materials, float)> list3 = new List<(Materials, float)>(num);
		CollectionsMarshal.SetCount(list3, num);
		Span<(Materials, float)> span3 = CollectionsMarshal.AsSpan(list3);
		num2 = 0;
		span3[num2] = (Materials.Nickel, 0.4f);
		num2++;
		span3[num2] = (Materials.Iron, 0.2f);
		dictionary3.Add(Materials.Platinum, list3);
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary4 = companionOresMap;
		num2 = 1;
		List<(Materials, float)> list4 = new List<(Materials, float)>(num2);
		CollectionsMarshal.SetCount(list4, num2);
		Span<(Materials, float)> span4 = CollectionsMarshal.AsSpan(list4);
		num = 0;
		span4[num] = (Materials.Zinc, 0.5f);
		dictionary4.Add(Materials.Lead, list4);
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary5 = companionOresMap;
		num = 2;
		List<(Materials, float)> list5 = new List<(Materials, float)>(num);
		CollectionsMarshal.SetCount(list5, num);
		Span<(Materials, float)> span5 = CollectionsMarshal.AsSpan(list5);
		num2 = 0;
		span5[num2] = (Materials.Lead, 0.5f);
		num2++;
		span5[num2] = (Materials.Copper, 0.3f);
		dictionary5.Add(Materials.Zinc, list5);
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary6 = companionOresMap;
		num2 = 1;
		List<(Materials, float)> list6 = new List<(Materials, float)>(num2);
		CollectionsMarshal.SetCount(list6, num2);
		Span<(Materials, float)> span6 = CollectionsMarshal.AsSpan(list6);
		num = 0;
		span6[num] = (Materials.Lead, 0.6f);
		dictionary6.Add(Materials.Stannary, list6);
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary7 = companionOresMap;
		num = 1;
		List<(Materials, float)> list7 = new List<(Materials, float)>(num);
		CollectionsMarshal.SetCount(list7, num);
		Span<(Materials, float)> span7 = CollectionsMarshal.AsSpan(list7);
		num2 = 0;
		span7[num2] = (Materials.Gold, 0.1f);
		dictionary7.Add(Materials.Mercury, list7);
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary8 = companionOresMap;
		num2 = 1;
		List<(Materials, float)> list8 = new List<(Materials, float)>(num2);
		CollectionsMarshal.SetCount(list8, num2);
		Span<(Materials, float)> span8 = CollectionsMarshal.AsSpan(list8);
		num = 0;
		span8[num] = (Materials.Iron, 0.3f);
		dictionary8.Add(Materials.Chromium, list8);
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary9 = companionOresMap;
		num = 1;
		List<(Materials, float)> list9 = new List<(Materials, float)>(num);
		CollectionsMarshal.SetCount(list9, num);
		Span<(Materials, float)> span9 = CollectionsMarshal.AsSpan(list9);
		num2 = 0;
		span9[num2] = (Materials.Iron, 0.3f);
		dictionary9.Add(Materials.Titanium, list9);
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary10 = companionOresMap;
		num2 = 2;
		List<(Materials, float)> list10 = new List<(Materials, float)>(num2);
		CollectionsMarshal.SetCount(list10, num2);
		Span<(Materials, float)> span10 = CollectionsMarshal.AsSpan(list10);
		num = 0;
		span10[num] = (Materials.Platinum, 0.1f);
		num++;
		span10[num] = (Materials.Copper, 0.3f);
		dictionary10.Add(Materials.Nickel, list10);
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary11 = companionOresMap;
		num = 1;
		List<(Materials, float)> list11 = new List<(Materials, float)>(num);
		CollectionsMarshal.SetCount(list11, num);
		Span<(Materials, float)> span11 = CollectionsMarshal.AsSpan(list11);
		num2 = 0;
		span11[num2] = (Materials.Phosphorus, 0.3f);
		dictionary11.Add(Materials.Uranium, list11);
		Dictionary<Materials, List<(Materials Ore, float Probability)>> dictionary12 = companionOresMap;
		num2 = 1;
		List<(Materials, float)> list12 = new List<(Materials, float)>(num2);
		CollectionsMarshal.SetCount(list12, num2);
		Span<(Materials, float)> span12 = CollectionsMarshal.AsSpan(list12);
		num = 0;
		span12[num] = (Materials.Titanium, 0.2f);
		dictionary12.Add(Materials.Phosphorus, list12);
		mineralProbabilities = new Dictionary<Materials, float>
		{
			{
				Materials.Gold,
				0.2f
			},
			{
				Materials.Silver,
				0.65f
			},
			{
				Materials.Platinum,
				0.23f
			},
			{
				Materials.Lead,
				2.25f
			},
			{
				Materials.Zinc,
				2.7f
			},
			{
				Materials.Stannary,
				1.75f
			},
			{
				Materials.Mercury,
				0.73f
			},
			{
				Materials.Chromium,
				3.05f
			},
			{
				Materials.Titanium,
				5f
			},
			{
				Materials.Nickel,
				2.9f
			},
			{
				Materials.Uranium,
				1.9f
			},
			{
				Materials.Phosphorus,
				4.3f
			},
			{
				Materials.Iron,
				2.6f
			}
		};
	}

	public static List<(Materials Ore, float Probability)> GetCompanionOres(Materials primaryOre)
	{
		if (companionOresMap.TryGetValue(primaryOre, out List<(Materials, float)> value))
		{
			return value;
		}
		throw new ArgumentException($"Companion ores for {primaryOre} not found.");
	}

	public static float GetOreProbability(Materials ore)
	{
		if (mineralProbabilities.TryGetValue(ore, out var value))
		{
			return value;
		}
		throw new ArgumentException($"Companion ores for {mineralProbabilities} not found.");
	}

	public static Materials PolymetallicNoduleDropOre(Game.Random random)
	{
		float num = random.Float();
		float num2 = 0f;
		foreach (KeyValuePair<Materials, float> item in polymetallicNoduleDropTable)
		{
			num2 += item.Value;
			if (num <= num2)
			{
				return item.Key;
			}
		}
		return Materials.Iron;
	}
}
