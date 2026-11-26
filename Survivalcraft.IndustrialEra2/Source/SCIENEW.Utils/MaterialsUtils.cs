using System;
using System.Collections.Generic;
using Engine;
using Game;
using SCIENEW.Extensions;

namespace SCIENEW.Utils;

public static class MaterialsUtils
{
	public static readonly Dictionary<Materials, int> materialFuelLevels = new Dictionary<Materials, int>
	{
		{
			Materials.None,
			0
		},
		{
			Materials.Steel,
			2
		},
		{
			Materials.Gold,
			1
		},
		{
			Materials.Silver,
			1
		},
		{
			Materials.Lead,
			1
		},
		{
			Materials.Platinum,
			3
		},
		{
			Materials.Zinc,
			1
		},
		{
			Materials.Stannary,
			1
		},
		{
			Materials.Chromium,
			3
		},
		{
			Materials.Titanium,
			3
		},
		{
			Materials.Nickel,
			2
		},
		{
			Materials.Aluminum,
			1
		},
		{
			Materials.Uranium,
			3
		},
		{
			Materials.Phosphorus,
			1
		},
		{
			Materials.Iron,
			2
		},
		{
			Materials.Copper,
			1
		},
		{
			Materials.Mercury,
			1
		},
		{
			Materials.Germanium,
			1
		},
		{
			Materials.FeAlCrAlloy,
			2
		},
		{
			Materials.Brass,
			1
		},
		{
			Materials.Plastic,
			1
		},
		{
			Materials.Vanadium,
			3
		},
		{
			Materials.Cupronickel,
			2
		}
	};

	public static readonly Dictionary<Materials, float> materialMeltingPoints = new Dictionary<Materials, float>
	{
		{
			Materials.None,
			0f
		},
		{
			Materials.Steel,
			1400f
		},
		{
			Materials.Gold,
			1064f
		},
		{
			Materials.Silver,
			961f
		},
		{
			Materials.Lead,
			327f
		},
		{
			Materials.Platinum,
			1768f
		},
		{
			Materials.Zinc,
			419f
		},
		{
			Materials.Stannary,
			232f
		},
		{
			Materials.Chromium,
			1907f
		},
		{
			Materials.Titanium,
			1885f
		},
		{
			Materials.Nickel,
			1455f
		},
		{
			Materials.Aluminum,
			660f
		},
		{
			Materials.Uranium,
			1132f
		},
		{
			Materials.Phosphorus,
			44f
		},
		{
			Materials.Iron,
			1538f
		},
		{
			Materials.Copper,
			1084f
		},
		{
			Materials.Mercury,
			-39f
		},
		{
			Materials.Germanium,
			938f
		},
		{
			Materials.FeAlCrAlloy,
			1350f
		},
		{
			Materials.Brass,
			920f
		},
		{
			Materials.Plastic,
			200f
		},
		{
			Materials.Vanadium,
			1910f
		},
		{
			Materials.Cupronickel,
			1200f
		}
	};

	public static readonly Dictionary<Materials, int> explosionResilience = new Dictionary<Materials, int>
	{
		{
			Materials.Germanium,
			80
		},
		{
			Materials.Iron,
			2000
		},
		{
			Materials.Copper,
			4000
		},
		{
			Materials.Steel,
			2500
		},
		{
			Materials.Gold,
			500
		},
		{
			Materials.Silver,
			600
		},
		{
			Materials.Lead,
			1200
		},
		{
			Materials.Platinum,
			1500
		},
		{
			Materials.Zinc,
			800
		},
		{
			Materials.Stannary,
			900
		},
		{
			Materials.Chromium,
			3500
		},
		{
			Materials.Titanium,
			4500
		},
		{
			Materials.Nickel,
			2500
		},
		{
			Materials.Aluminum,
			700
		},
		{
			Materials.Uranium,
			5000
		},
		{
			Materials.Phosphorus,
			300
		},
		{
			Materials.FeAlCrAlloy,
			4000
		},
		{
			Materials.Brass,
			4000
		},
		{
			Materials.Plastic,
			100
		},
		{
			Materials.Vanadium,
			3500
		},
		{
			Materials.Cupronickel,
			3000
		}
	};

	public static readonly Dictionary<Materials, int> digResilience = new Dictionary<Materials, int>
	{
		{
			Materials.Germanium,
			10
		},
		{
			Materials.Iron,
			24
		},
		{
			Materials.Copper,
			24
		},
		{
			Materials.Steel,
			30
		},
		{
			Materials.Gold,
			5
		},
		{
			Materials.Silver,
			8
		},
		{
			Materials.Lead,
			12
		},
		{
			Materials.Platinum,
			20
		},
		{
			Materials.Zinc,
			15
		},
		{
			Materials.Stannary,
			15
		},
		{
			Materials.Chromium,
			35
		},
		{
			Materials.Titanium,
			40
		},
		{
			Materials.Nickel,
			30
		},
		{
			Materials.Aluminum,
			20
		},
		{
			Materials.Uranium,
			35
		},
		{
			Materials.Phosphorus,
			10
		},
		{
			Materials.FeAlCrAlloy,
			40
		},
		{
			Materials.Brass,
			25
		},
		{
			Materials.Plastic,
			3
		},
		{
			Materials.Vanadium,
			30
		},
		{
			Materials.Cupronickel,
			28
		}
	};

	public static Color GetColor(Materials type)
	{
		switch (type)
		{
		case Materials.Steel:
		case Materials.Phosphorus:
			return Color.LightGray;
		case Materials.Gold:
			return new Color(255, 215, 0);
		case Materials.Lead:
			return new Color(88, 87, 86);
		case Materials.Platinum:
			return new Color(253, 253, 253);
		case Materials.Chromium:
			return new Color(58, 57, 56);
		case Materials.Iron:
			return Color.White;
		case Materials.Copper:
			return new Color(255, 127, 80);
		case Materials.FeAlCrAlloy:
			return new Color(200, 200, 200);
		case Materials.Titanium:
			return Color.DarkGray;
		case Materials.Brass:
			return new Color(250, 160, 30);
		case Materials.Cupronickel:
			return new Color(228, 223, 200);
		default:
			return new Color(232, 232, 232);
		}
	}

	public static string ToDisplayName(Materials materials)
	{
		return LanguageControl.Get("Materials", materials.ToString());
	}

	public static string GetAlloyDisplayName(string alloyID)
	{
		return LanguageControl.Get("Alloys", alloyID);
	}

	public static int GetRequiredFuelLevel(Materials material)
	{
		if (!materialFuelLevels.TryGetValue(material, out var _))
		{
			return 0;
		}
		return materialFuelLevels[material];
	}

	public static int GetExplosionResilience(Materials material)
	{
		return explosionResilience[material];
	}

	public static int GetDigResilience(Materials material)
	{
		return digResilience[material];
	}

	public static Color GetMetalColor(Materials material)
	{
		Color color = materialMeltingPoints[material].BlackbodyColor();
		double ratio = Math.Clamp((materialMeltingPoints[material] - 1000f) / 6000f, 0f, 1f);
		return GetColor(material).BlendColors(color, ratio);
	}
}
