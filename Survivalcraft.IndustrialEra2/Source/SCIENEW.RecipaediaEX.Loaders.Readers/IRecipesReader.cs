using System.Xml.Linq;
using RecipaediaEX;

namespace SCIENEW.RecipaediaEX.Loaders.Readers;

public interface IRecipesReader
{
	string Identifier { get; }

	IRecipe? Decode(XElement element);
}
