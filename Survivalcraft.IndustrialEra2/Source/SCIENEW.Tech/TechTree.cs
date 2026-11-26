using System;
using System.Collections.Generic;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW.Tech;

public class TechTree
{
	public Dictionary<string, TechNode> TechNodes { get; } = new Dictionary<string, TechNode>();

	public string Name { get; protected set; }

	public string Id { get; protected set; }

	public TechTree(string name, string? id = null)
	{
		Name = (string.IsNullOrWhiteSpace(name) ? "Default" : name.Trim());
		Id = (string.IsNullOrWhiteSpace(id) ? TechTreeManager.DeriveTechTreeIdFromName(Name) : TechTreeManager.SanitizeTechTreeId(id));
	}

	public void SetTreeName(string name)
	{
		Name = (string.IsNullOrWhiteSpace(name) ? "Default" : name.Trim());
	}

	public void SetTreeId(string id)
	{
		Id = (string.IsNullOrWhiteSpace(id) ? TechTreeManager.DeriveTechTreeIdFromName(Name) : TechTreeManager.SanitizeTechTreeId(id));
	}

	public bool RenameNode(string oldName, string newName)
	{
		if (string.IsNullOrWhiteSpace(newName))
		{
			return false;
		}
		if (!TechNodes.TryGetValue(oldName, out TechNode value))
		{
			return false;
		}
		string text = newName.Trim();
		if (oldName == text)
		{
			return true;
		}
		if (TechNodes.ContainsKey(text))
		{
			return false;
		}
		TechNodes.Remove(oldName);
		value.RenameTo(text);
		TechNodes[text] = value;
		return true;
	}

	public void RemoveNode(string name)
	{
		if (!TechNodes.TryGetValue(name, out TechNode removed))
		{
			return;
		}
		TechNodes.Remove(name);
		foreach (TechNode item in TechNodes.Values.AsValueEnumerable().ToList())
		{
			item.Dependencies.RemoveAll((TechNode d) => d == removed || d.Name == name);
		}
	}

	public void RemoveDependency(string techName, string dependencyName)
	{
		if (TechNodes.TryGetValue(techName, out TechNode value))
		{
			value.Dependencies.RemoveAll((TechNode d) => d.Name == dependencyName);
		}
	}

	public TechNode GetOrCreateNode(string name)
	{
		if (!TechNodes.TryGetValue(name, out TechNode value))
		{
			value = new TechNode(name);
			TechNodes[name] = value;
		}
		return value;
	}

	public void AddDependency(string techName, string dependencyName)
	{
		TechNode orCreateNode = GetOrCreateNode(techName);
		TechNode orCreateNode2 = GetOrCreateNode(dependencyName);
		orCreateNode.AddDependency(orCreateNode2);
	}

	public void AddDependency(string techName, IEnumerable<string> dependenciesNames)
	{
		TechNode orCreateNode = GetOrCreateNode(techName);
		foreach (string dependenciesName in dependenciesNames)
		{
			TechNode orCreateNode2 = GetOrCreateNode(dependenciesName);
			orCreateNode.AddDependency(orCreateNode2);
		}
	}

	public TechNode GetNode(string name)
	{
		TechNodes.TryGetValue(name, out TechNode value);
		return value;
	}

	public TechNode[] GetNodeByStage(int stage)
	{
		return (from x in TechNodes.Values.AsValueEnumerable()
			where x.Stage == stage
			select x).OrderBy<Where<FromEnumerable<TechNode>, TechNode>, TechNode, int>((Func<TechNode, int>)((TechNode x) => x.StageOrder)).ThenBy<Where<FromEnumerable<TechNode>, TechNode>, TechNode, int, string>((Func<TechNode, string>)((TechNode x) => x.Name), (IComparer<string>?)StringComparer.Ordinal).ToArray<OrderBy<Where<FromEnumerable<TechNode>, TechNode>, TechNode, string>, TechNode>();
	}

	public int AllocateStageOrderForStage(int stage)
	{
		return (from n in TechNodes.Values.AsValueEnumerable()
			where n.Stage == stage
			select n).Select<FromEnumerable<TechNode>, TechNode, int>((Func<TechNode, int>)((TechNode n) => n.StageOrder)).DefaultIfEmpty<WhereSelect<FromEnumerable<TechNode>, TechNode, int>, int>(-1).Max<DefaultIfEmpty<WhereSelect<FromEnumerable<TechNode>, TechNode, int>, int>, int>() + 1;
	}

	public bool MoveNodeStageOrderWithinStage(string nodeName, bool moveEarlier)
	{
		if (!TechNodes.TryGetValue(nodeName, out TechNode value))
		{
			return false;
		}
		int stage = value.Stage;
		List<TechNode> list = (from n in TechNodes.Values.AsValueEnumerable()
			where n.Stage == stage
			select n).OrderBy<Where<FromEnumerable<TechNode>, TechNode>, TechNode, int>((Func<TechNode, int>)((TechNode n) => n.StageOrder)).ThenBy<Where<FromEnumerable<TechNode>, TechNode>, TechNode, int, string>((Func<TechNode, string>)((TechNode n) => n.Name), (IComparer<string>?)StringComparer.Ordinal).ToList<OrderBy<Where<FromEnumerable<TechNode>, TechNode>, TechNode, string>, TechNode>();
		int num = list.IndexOf(value);
		if (num < 0)
		{
			return false;
		}
		int num2 = (moveEarlier ? (num - 1) : (num + 1));
		if (num2 < 0 || num2 >= list.Count)
		{
			return false;
		}
		List<TechNode> list2 = list;
		int index = num;
		int index2 = num2;
		TechNode value2 = list[num2];
		TechNode value3 = list[num];
		list2[index] = value2;
		list[index2] = value3;
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			list[num3].StageOrder = num3;
		}
		return true;
	}

	public TechNode[] GetUnlockedNodes()
	{
		return (from x in TechNodes.Values.AsValueEnumerable()
			where !x.IsLocked
			select x).ToArray<FromEnumerable<TechNode>, TechNode>();
	}

	public bool UnlockNode(string name, out TechNode unlockedNode)
	{
		unlockedNode = null;
		foreach (KeyValuePair<string, TechNode> techNode in TechNodes)
		{
			if (techNode.Key == name)
			{
				bool result = !techNode.Value.IsLocked;
				techNode.Value.IsLocked = false;
				unlockedNode = techNode.Value;
				return result;
			}
		}
		return false;
	}

	public bool UnlockNode(int iconBlock, out TechNode unlockedNode)
	{
		unlockedNode = null;
		foreach (KeyValuePair<string, TechNode> techNode in TechNodes)
		{
			if (techNode.Value.IconBlock == iconBlock)
			{
				bool result = !techNode.Value.IsLocked;
				techNode.Value.IsLocked = false;
				unlockedNode = techNode.Value;
				return result;
			}
		}
		return false;
	}

	public void UnlockNodes(IEnumerable<string> nodes)
	{
		foreach (string node in nodes)
		{
			UnlockNode(node, out TechNode _);
		}
	}

	public void LockAllNodes()
	{
		foreach (KeyValuePair<string, TechNode> techNode in TechNodes)
		{
			techNode.Value.IsLocked = true;
		}
	}
}
