using Engine;
using SCIENEW.Utils;

namespace SCIENEW.Extensions.GameLogic;

public static class MaterialsExtensions
{
	public static Color GetColor(this Materials type)
	{
		return MaterialsUtils.GetColor(type);
	}

	public static string ToDisplayName(this Materials materials)
	{
		return MaterialsUtils.ToDisplayName(materials);
	}

	public static int GetRequiredFuelLevel(this Materials material)
	{
		return MaterialsUtils.GetRequiredFuelLevel(material);
	}

	public static int GetExplosionResilience(this Materials material)
	{
		return MaterialsUtils.GetExplosionResilience(material);
	}

	public static int GetDigResilience(this Materials material)
	{
		return MaterialsUtils.GetDigResilience(material);
	}

	public static Color GetMetalColor(this Materials material)
	{
		return MaterialsUtils.GetMetalColor(material);
	}
}
