using System.Xml.Linq;

namespace SCIENEW.Tech;

public sealed class TechTreeModFile
{
	public string PackageName { get; }

	public string ModTitle { get; }

	public string FileName { get; }

	public XElement Root { get; }

	public TechTreeModFile(string packageName, string modTitle, string fileName, XElement root)
	{
		PackageName = packageName;
		ModTitle = modTitle;
		FileName = fileName;
		Root = root;
	}
}
