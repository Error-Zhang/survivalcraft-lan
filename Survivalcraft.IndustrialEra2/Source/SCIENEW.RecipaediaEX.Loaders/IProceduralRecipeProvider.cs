using System.Collections.Generic;
using RecipaediaEX;

namespace SCIENEW.RecipaediaEX.Loaders;

public interface IProceduralRecipeProvider
{
	IEnumerable<IRecipe> GetProceduralRecipes();
}
