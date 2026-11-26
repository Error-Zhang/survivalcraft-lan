using System;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Implementation;
using SCIENEW.RecipaediaEX;
using SCIENEW.Utils;

namespace SCIENEW;

public class MagazineItem : MeshItem
{
	public int m_bulletCount;

	public bool m_display;

	public const int MaxBullet = 63;

	public MagazineItem(string idFront, int bulletCount, bool display)
		: base(idFront + bulletCount, "Models/Magazine", "charger", ContentManager.Get<Texture2D>("Textures/Magazine"), Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f) * Matrix.CreateRotationY((float)Math.PI), Matrix.Identity, 2.4f)
	{
		m_bulletCount = bulletCount;
		m_display = display;
		if (m_bulletCount > 63)
		{
			throw new ArgumentOutOfRangeException("bulletCount", bulletCount, "Max bullet count is " + 63);
		}
	}

	public override bool IsDisplay()
	{
		return m_display;
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return string.Format(LanguageControl.Get("Items", "Names", "MagazineItem"), m_bulletCount);
	}

	public override string GetDescription(int value)
	{
		return LanguageControl.Get("Items", "Descriptions", "MagazineItem");
	}

	public override IRecipe? GetDynamicRecipe(IRecipe actual, Project project)
	{
		if (!DynamicRecipeUtils.TryResolveCraftingContext(actual, project, out SubsystemTerrain _, out FormattedRecipe formattedRecipe, out string[] _, out float heatLevel, out float playerLevel))
		{
			return null;
		}
		string[] craftingIngredientStrings = DynamicRecipeUtils.GetCraftingIngredientStrings(formattedRecipe, 0f);
		if (craftingIngredientStrings == null)
		{
			return null;
		}
		int? num = null;
		int num2 = 0;
		for (int i = 0; i < craftingIngredientStrings.Length; i++)
		{
			if (string.IsNullOrEmpty(craftingIngredientStrings[i]))
			{
				continue;
			}
			RecipesUtils.DecodeIEIngredient(craftingIngredientStrings[i], out string craftingId, out int? _);
			if (craftingId.StartsWith("I-") && ItemsBlockManager.GetItem(craftingId.Substring(2, craftingId.Length - 2)) is MagazineItem magazineItem)
			{
				if (num.HasValue)
				{
					return null;
				}
				num = magazineItem.m_bulletCount;
			}
			else
			{
				if (!craftingId.StartsWith("I-") || !(craftingId.Substring(2, craftingId.Length - 2) == "StandardBullet"))
				{
					return null;
				}
				num2++;
			}
		}
		if (!num.HasValue)
		{
			return null;
		}
		if (num.Value + num2 > 63)
		{
			return null;
		}
		foreach (MagazineItem item in ItemsBlockManager.GetItems<MagazineItem>())
		{
			if (item.m_bulletCount == num.Value + num2)
			{
				return DynamicRecipeUtils.FromCraftingRecipe(new CraftingRecipe
				{
					Ingredients = craftingIngredientStrings,
					RequiredHeatLevel = heatLevel,
					RequiredPlayerLevel = playerLevel,
					ResultCount = 1,
					ResultValue = ItemsBlockManager.GetItemID(item)
				});
			}
		}
		return null;
	}
}
