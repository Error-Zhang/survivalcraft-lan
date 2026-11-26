using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MiniBinaryXml;

public class MiniBinaryXmlWriter
{
	private class Node
	{
		public int TagNameIdx;

		public int InnerTextIdx;

		public int ParentIdx;

		public List<(int nameIdx, int valueIdx)> Attributes = new List<(int, int)>();

		public List<int> Children = new List<int>();
	}

	private List<string> stringPool = new List<string>();

	private Dictionary<string, int> stringIndex = new Dictionary<string, int>();

	private List<Node> nodes = new List<Node>();

	private int AddString(string s)
	{
		if (stringIndex.TryGetValue(s, out var value))
		{
			return value;
		}
		value = stringPool.Count;
		stringPool.Add(s);
		stringIndex[s] = value;
		return value;
	}

	public void Write(XElement doc, Stream stream)
	{
		nodes.Clear();
		stringPool.Clear();
		stringIndex.Clear();
		Traverse(doc, -1);
		using BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
		binaryWriter.Write(Encoding.ASCII.GetBytes("MBXM"));
		binaryWriter.Write((byte)1);
		binaryWriter.Write(new byte[3]);
		binaryWriter.Write(stringPool.Count);
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter2 = new BinaryWriter(memoryStream);
		List<int> list = new List<int>();
		int num = 0;
		foreach (string item in stringPool)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(item);
			list.Add(num);
			binaryWriter2.Write((ushort)bytes.Length);
			binaryWriter2.Write(bytes);
			num += 2 + bytes.Length;
		}
		foreach (int item2 in list)
		{
			binaryWriter.Write(item2);
		}
		binaryWriter.Write(memoryStream.ToArray());
		binaryWriter.Write(nodes.Count);
		foreach (Node node in nodes)
		{
			binaryWriter.Write(node.TagNameIdx);
			binaryWriter.Write(node.InnerTextIdx);
			binaryWriter.Write(node.ParentIdx);
			binaryWriter.Write((ushort)node.Attributes.Count);
			binaryWriter.Write((ushort)node.Children.Count);
			foreach (var (value, value2) in node.Attributes)
			{
				binaryWriter.Write(value);
				binaryWriter.Write(value2);
			}
			foreach (int child in node.Children)
			{
				binaryWriter.Write(child);
			}
		}
	}

	private int Traverse(XElement elem, int parentIdx)
	{
		Node node = new Node
		{
			TagNameIdx = AddString(elem.Name.ToString()),
			ParentIdx = parentIdx
		};
		foreach (XAttribute item2 in elem.Attributes())
		{
			node.Attributes.Add((AddString(item2.Name.ToString()), AddString(item2.Value)));
		}
		node.InnerTextIdx = (string.IsNullOrEmpty(GetDirectText(elem)) ? (-1) : AddString(GetDirectText(elem)));
		int count = nodes.Count;
		nodes.Add(node);
		foreach (XElement item3 in elem.Elements())
		{
			int item = Traverse(item3, count);
			node.Children.Add(item);
		}
		return count;
	}

	private string GetDirectText(XElement element)
	{
		return string.Concat(from t in element.Nodes().OfType<XText>()
			select t.Value);
	}
}
