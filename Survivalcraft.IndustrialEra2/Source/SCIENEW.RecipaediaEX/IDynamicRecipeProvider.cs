using GameEntitySystem;
using RecipaediaEX;

namespace SCIENEW.RecipaediaEX;

public interface IDynamicRecipeProvider
{
	IRecipe? GetDynamicRecipe(IRecipe actual, Project project);
}
