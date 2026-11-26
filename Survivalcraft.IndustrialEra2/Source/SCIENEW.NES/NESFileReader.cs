using Game;
using Game.IContentReader;

namespace SCIENEW.NES;

public class NESFileReader : IContentReader
{
	public override string Type => "SCIENEW.NES.NESFile";

	public override string[] DefaultSuffix => new string[1] { "nes" };

	public override object Get(ContentInfo[] contents)
	{
		return new NESFile(ModsManager.StreamToBytes(contents[0].Duplicate()), contents[0].Filename);
	}
}
