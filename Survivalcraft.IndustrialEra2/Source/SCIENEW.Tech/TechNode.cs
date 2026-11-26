using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;

namespace SCIENEW.Tech;

public class TechNode
{
	public enum Mode
	{
		Locked,
		Unlocked,
		Handling
	}

	public string Name { get; private set; }

	public List<TechNode> Dependencies { get; private set; }

	public string DisplayName { get; set; }

	public string Description { get; set; }

	public int IconBlock { get; set; }

	public bool IsLocked { get; set; }

	public List<BlockDropValue> Rewards { get; set; }

	public int Stage { get; set; }

	public int StageOrder { get; set; }

	public Vector2 LayoutOffset { get; set; }

	internal void RenameTo(string newName)
	{
		if (string.IsNullOrWhiteSpace(newName))
		{
			throw new ArgumentException("Node name cannot be empty.", "newName");
		}
		Name = newName.Trim();
	}

	public TechNode(string name)
	{
		Name = name;
		Dependencies = new List<TechNode>();
		IsLocked = true;
		Rewards = new List<BlockDropValue>();
		Stage = 0;
	}

	public void AddDependency(TechNode node)
	{
		Dependencies.Add(node);
	}

	public Mode GetMode()
	{
		if (IsLocked && Dependencies.All((TechNode x) => x.IsLocked) && Dependencies.Count > 0)
		{
			return Mode.Locked;
		}
		if (IsLocked && (Dependencies.Any((TechNode x) => !x.IsLocked) || Dependencies.Count == 0))
		{
			return Mode.Handling;
		}
		if (!IsLocked)
		{
			return Mode.Unlocked;
		}
		return Mode.Locked;
	}
}
