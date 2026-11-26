using System;
using Game;

namespace SCIENEW;

public abstract class DefaultItem : ItemBlock
{
	public string Category = "Items";

	public string Id = string.Empty;

	public string Description = string.Empty;

	public string DisplayName = string.Empty;

	public DefaultItem(string id)
	{
		Id = id;
		try
		{
			DisplayName = LanguageControl.Get("Items", "Names", GetCraftingId());
			Description = LanguageControl.Get("Items", "Descriptions", GetCraftingId());
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
