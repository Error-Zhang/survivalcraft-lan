using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;

namespace SCIENEW.NES;

public static class NESManager
{
	public static Dictionary<string, NESFile> m_nesFiles = new Dictionary<string, NESFile>();

	public static Dictionary<string, NESFile> NesFiles => m_nesFiles;

	public static void Init()
	{
		ReadOnlyList<ContentInfo> readOnlyList = ContentManager.List("Nes");
		NESFileReader nESFileReader = new NESFileReader();
		foreach (ContentInfo item in readOnlyList)
		{
			string filename = item.Filename;
			if (nESFileReader.Get(new ContentInfo[1] { item }) is NESFile value)
			{
				m_nesFiles.TryAdd(filename, value);
			}
		}
		m_nesFiles.Add("NesTest", ContentManager.Get<NESFile>("Nestest"));
		m_nesFiles = m_nesFiles.OrderBy((KeyValuePair<string, NESFile> x) => x.Key).ToDictionary((KeyValuePair<string, NESFile> x) => x.Key, (KeyValuePair<string, NESFile> x) => x.Value);
	}

	public static bool TryGet(string name, out NESFile nesFile)
	{
		return m_nesFiles.TryGetValue(name, out nesFile);
	}
}
