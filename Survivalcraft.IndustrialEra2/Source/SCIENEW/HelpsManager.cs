using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using MiniBinaryXml;
using SCIENEW.Utils;
using XmlUtilities;

namespace SCIENEW;

public static class HelpsManager
{
	public static Dictionary<string, string> m_blockToLink = new Dictionary<string, string>();

	public static void Init()
	{
		XElement hlp = new XElement("Helps");
		IndustrialModLoader.GetFile("Helps.qft", (Stream stream) =>
		{
			hlp = new MiniBinaryXmlReader().ReadAsXml(stream);
		});
		m_blockToLink = DecodeHelpFile(hlp);
	}

	public static Dictionary<string, string> DecodeHelpFile(XElement hlp)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (XElement item in hlp.Elements())
		{
			string attributeValue = XmlUtils.GetAttributeValue<string>(item, "ID");
			string attributeValue2 = XmlUtils.GetAttributeValue<string>(item, "Link");
			string[] array = attributeValue.Split(';');
			foreach (string key in array)
			{
				dictionary[key] = attributeValue2;
			}
		}
		return dictionary;
	}

	public static bool GetLink(int blockValue, out string? link)
	{
		string key = RecipesUtils.BlockToCratingID(blockValue);
		return m_blockToLink.TryGetValue(key, out link);
	}
}
