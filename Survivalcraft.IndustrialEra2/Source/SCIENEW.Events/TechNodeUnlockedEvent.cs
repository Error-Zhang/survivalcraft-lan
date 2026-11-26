using Game;
using GameEntitySystem;
using SCIENEW.Tech;

namespace SCIENEW.Events;

public readonly struct TechNodeUnlockedEvent(Project project, TechTree techTree, TechNode node, int blockValue, ComponentPlayer? player)
{
	public Project Project { get; } = project;

	public TechTree TechTree { get; } = techTree;

	public TechNode Node { get; } = node;

	public int BlockValue { get; } = blockValue;

	public ComponentPlayer? Player { get; } = player;
}
