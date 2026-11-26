using System.Collections.Generic;
using RecipaediaEX.UI;

namespace SCIENEW.RecipaediaEX.Categories;

public class FluidsCategoryProvider : IRecipaediaCategoryProvider
{
	public IEnumerable<IRecipaediaCategory> GetCategories()
	{
		return new _003C_003Ez__ReadOnlySingleElementList<IRecipaediaCategory>(new FluidCategory());
	}
}
