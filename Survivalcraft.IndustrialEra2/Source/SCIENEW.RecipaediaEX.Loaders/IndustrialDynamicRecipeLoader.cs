using Game;
using GameEntitySystem;
using RecipaediaEX;

namespace SCIENEW.RecipaediaEX.Loaders;

public class IndustrialDynamicRecipeLoader : IDynamicRecipeLoader
{
	public int Order => 10;

	public void Initialize()
	{
	}

	public IRecipe GetDynamicRecipe(IRecipe actual, Project project)
	{
		Block[] blocks = BlocksManager.Blocks;
		for (int i = 0; i < blocks.Length; i++)
		{
			IRecipe industrialDynamicRecipe = blocks[i].GetIndustrialDynamicRecipe(actual, project);
			if (industrialDynamicRecipe != null && industrialDynamicRecipe.Match(actual))
			{
				return industrialDynamicRecipe;
			}
		}
		return null;
	}
}
