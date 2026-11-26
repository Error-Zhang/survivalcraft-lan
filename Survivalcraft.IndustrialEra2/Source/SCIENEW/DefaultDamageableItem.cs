using System;
using Game;

namespace SCIENEW;

public abstract class DefaultDamageableItem : DamageableItemBlock
{
	public string DisplayName = string.Empty;

	public string Description = string.Empty;

	public string Category = "Items";

	public string Id = string.Empty;

	public DefaultDamageableItem()
	{
	}

	public DefaultDamageableItem(string id)
	{
		Id = id;
		try
		{
			DisplayName = LanguageControl.Get("DamageableBlocks", "Names", GetCraftingId());
			Description = LanguageControl.Get("DamageableBlocks", "Descriptions", GetCraftingId());
		}
		catch (NullReferenceException)
		{
		}
	}

	public override string GetDescription(int value)
	{
		return Description;
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return DisplayName;
	}

	public override string GetCategory(int value)
	{
		return Category;
	}

	public override string GetCraftingId()
	{
		return Id;
	}
}
