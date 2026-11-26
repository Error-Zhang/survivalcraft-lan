using System;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;

namespace SCIENEW;

public class HookDItemBehavior : DamageableItemBehavior
{
	public HookDItemBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		TerrainRaycastResult? raycastResult = Raycast(ray, RaycastMode.Digging, componentMiner);
		ComponentHook componentHook = componentMiner.Entity.FindComponent<ComponentHook>();
		if (ElectricToolDItem.IsOutOfPower(componentMiner.ActiveBlockValue))
		{
			return false;
		}
		if (componentHook != null)
		{
			componentHook.Hook(raycastResult, out var hookDistance);
			DamageableItemManager.DamageActiveTool(componentMiner, Math.Clamp((int)(hookDistance / 2f), 1, 50));
		}
		return false;
	}

	public TerrainRaycastResult? Raycast(Ray3 ray, RaycastMode mode, ComponentMiner componentMiner)
	{
		float reach = 64f;
		Vector3 creaturePosition = componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition;
		Vector3 start = ray.Position;
		Vector3 direction = ray.Direction.Normalize();
		Vector3 end = ray.Position + direction * reach;
		Point3 startCell = Terrain.ToCell(start);
		return componentMiner.m_subsystemTerrain.Raycast(start, end, useInteractionBoxes: true, skipAirBlocks: true, (int value, float distance) =>
		{
			if ((double)(start + distance * direction).DistanceSquared(creaturePosition) <= (double)reach * (double)reach)
			{
				Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
				if ((double)distance == 0.0 && block is CrossBlock && (double)direction.Dot(new Vector3(startCell) + new Vector3(0.5f) - start) < 0.0)
				{
					return false;
				}
				switch (mode)
				{
				case RaycastMode.Digging:
					return !block.IsDiggingTransparent;
				case RaycastMode.Interaction:
					if (block.IsPlacementTransparent_(value))
					{
						return block.IsInteractive(componentMiner.m_subsystemTerrain, value);
					}
					return true;
				case RaycastMode.Gathering:
					return block.IsGatherable_(value);
				}
			}
			return false;
		});
	}
}
