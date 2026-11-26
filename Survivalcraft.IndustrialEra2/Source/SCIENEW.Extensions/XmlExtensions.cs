using System;
using System.Xml.Linq;
using XmlUtilities;

namespace SCIENEW.Extensions;

public static class XmlExtensions
{
	extension(XElement node)
	{
		public object GetAttributeValue(string attributeName, Type type)
		{
			return XmlUtils.GetAttributeValue(node, attributeName, type);
		}

		public object GetAttributeValue(string attributeName, Type type, object defaultValue)
		{
			return XmlUtils.GetAttributeValue(node, attributeName, type, defaultValue);
		}

		public T GetAttributeValue<T>(string attributeName)
		{
			return XmlUtils.GetAttributeValue<T>(node, attributeName);
		}

		public T GetAttributeValue<T>(string attributeName, T defaultValue)
		{
			return XmlUtils.GetAttributeValue(node, attributeName, defaultValue);
		}

		public void SetAttributeValue(string attributeName, object value)
		{
			XmlUtils.SetAttributeValue(node, attributeName, value);
		}

		public XElement FindChildElement(string elementName, bool throwIfNotFound)
		{
			return XmlUtils.FindChildElement(node, elementName, throwIfNotFound);
		}

		public XElement AddElement(string name)
		{
			return XmlUtils.AddElement(node, name);
		}
	}
}
