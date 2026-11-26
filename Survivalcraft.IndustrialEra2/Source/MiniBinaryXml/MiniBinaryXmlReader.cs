using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MiniBinaryXml;

public class MiniBinaryXmlReader
{
	public class Node
	{
		public string Tag;

		public string InnerText;

		public Dictionary<string, string> Attributes = new Dictionary<string, string>();

		public List<Node> Children = new List<Node>();
	}

	public Node Read(Stream stream)
	{
		using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
		byte[] bytes = binaryReader.ReadBytes(4);
		if (Encoding.ASCII.GetString(bytes) != "MBXM")
		{
			throw new Exception("Invalid format");
		}
		binaryReader.ReadByte();
		binaryReader.ReadBytes(3);
		int num = binaryReader.ReadInt32();
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = binaryReader.ReadInt32();
		}
		long position = stream.Position;
		List<string> list = new List<string>();
		for (int j = 0; j < num; j++)
		{
			stream.Position = position + array[j];
			ushort count = binaryReader.ReadUInt16();
			byte[] bytes2 = binaryReader.ReadBytes(count);
			list.Add(Encoding.UTF8.GetString(bytes2));
		}
		stream.Position = position + array[^1];
		ushort num2 = binaryReader.ReadUInt16();
		binaryReader.ReadBytes(num2);
		stream.Position = position + array[^1] + 2 + num2;
		int num3 = binaryReader.ReadInt32();
		List<(int, int, int, List<(int, int)>, List<int>)> list2 = new List<(int, int, int, List<(int, int)>, List<int>)>();
		for (int k = 0; k < num3; k++)
		{
			int item = binaryReader.ReadInt32();
			int item2 = binaryReader.ReadInt32();
			int item3 = binaryReader.ReadInt32();
			ushort num4 = binaryReader.ReadUInt16();
			ushort num5 = binaryReader.ReadUInt16();
			List<(int, int)> list3 = new List<(int, int)>();
			List<int> list4 = new List<int>();
			for (int l = 0; l < num4; l++)
			{
				list3.Add((binaryReader.ReadInt32(), binaryReader.ReadInt32()));
			}
			for (int m = 0; m < num5; m++)
			{
				list4.Add(binaryReader.ReadInt32());
			}
			list2.Add((item, item2, item3, list3, list4));
		}
		Node[] array2 = new Node[num3];
		for (int n = 0; n < num3; n++)
		{
			Node node = new Node
			{
				Tag = list[list2[n].Item1]
			};
			if (list2[n].Item2 >= 0)
			{
				node.InnerText = list[list2[n].Item2];
			}
			foreach (var (index, index2) in list2[n].Item4)
			{
				node.Attributes[list[index]] = list[index2];
			}
			array2[n] = node;
		}
		for (int num6 = 0; num6 < num3; num6++)
		{
			foreach (int item4 in list2[num6].Item5)
			{
				array2[num6].Children.Add(array2[item4]);
			}
		}
		for (int num7 = 0; num7 < num3; num7++)
		{
			if (list2[num7].Item3 == -1)
			{
				return array2[num7];
			}
		}
		throw new Exception("No root node found");
	}

	public XElement ReadAsXml(Stream stream)
	{
		Node node = Read(stream);
		return BuildXml(node);
	}

	private XElement BuildXml(Node node)
	{
		XElement xElement = new XElement(XName.Get(node.Tag));
		if (!string.IsNullOrEmpty(node.InnerText))
		{
			SetDirectText(xElement, node.InnerText);
		}
		foreach (KeyValuePair<string, string> attribute in node.Attributes)
		{
			xElement.SetAttributeValue(XName.Get(attribute.Key), attribute.Value);
		}
		foreach (Node child in node.Children)
		{
			xElement.Add(BuildXml(child));
		}
		return xElement;
	}

	private void SetDirectText(XElement element, string newText)
	{
		foreach (XText item in element.Nodes().OfType<XText>().ToList())
		{
			item.Remove();
		}
		element.AddFirst(new XText(newText));
	}
}
