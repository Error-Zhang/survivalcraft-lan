using Game;
using GameEntitySystem;
using RecipaediaEX;

namespace SCIENEW.RecipaediaEX;

public static class DynamicRecipeBlockExtensions
{
	public static IRecipe? GetIndustrialDynamicRecipe(this Block block, IRecipe actual, Project project)
	{
		if (!(block is BaseItemBlock baseItemBlock))
		{
			if (!(block is BaseDeviceBlock baseDeviceBlock))
			{
				if (!(block is BaseNormalBlock baseNormalBlock))
				{
					if (block is BaseDamageableItemBlock baseDamageableItemBlock)
					{
						return baseDamageableItemBlock.GetDynamicRecipe(actual, project);
					}
					return null;
				}
				return baseNormalBlock.GetDynamicRecipe(actual, project);
			}
			return baseDeviceBlock.GetDynamicRecipe(actual, project);
		}
		return baseItemBlock.GetDynamicRecipe(actual, project);
	}
}
