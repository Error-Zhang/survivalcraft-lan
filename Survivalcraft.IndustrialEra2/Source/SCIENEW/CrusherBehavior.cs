using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CrusherBehavior : DeviceBehavior
{
	public SubsystemPickables m_subsystemPickables;

	public SubsystemProjectiles m_subsystemProjectiles;

	public SubsystemEnginePower m_subsystemEnginePower;

	public SubsystemTerrain m_SubsystemTerrain;

	public float m_speed = 20f;

	public CrusherBehavior(Project project)
		: base(project)
	{
		m_subsystemPickables = project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_subsystemProjectiles = project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
		m_subsystemEnginePower = project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_SubsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
	{
		if (worldItem.ToRemove || !m_subsystemEnginePower.IsPowered(cellFace.Point, out IEngineComponent _) || worldItem.Velocity.Length() < 20f)
		{
			return;
		}
		Vector3 vector = CellFace.FaceToVector3(cellFace.Face);
		if (((worldItem as Pickable)?.Count ?? 1) > 1)
		{
			return;
		}
		Vector3 position = new Vector3(cellFace.Point) + new Vector3(0.5f) - 0.75f * vector;
		if (Terrain.ExtractContents(worldItem.Value) == GravelBlock.Index)
		{
			for (int i = 0; i < 5; i++)
			{
				if (m_subsystemProjectiles.FireProjectile(StoneChunkBlock.Index, position, (0f - m_speed) * vector, Vector3.Zero, null) == null)
				{
					m_subsystemPickables.AddPickable(StoneChunkBlock.Index, 1, position, 1f * vector, null);
				}
			}
			worldItem.ToRemove = true;
			return;
		}
		List<BlockDropValue> list = new List<BlockDropValue>(8);
		BlocksManager.Blocks[Terrain.ExtractContents(worldItem.Value)].GetDropValues(m_SubsystemTerrain, worldItem.Value, 0, 3, list, out var _);
		if (list.Count == 0)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			BlockDropValue blockDropValue = list[i];
			int num = 0;
			if (list.Count > 1)
			{
				num = 1;
			}
			for (int j = 0; j < blockDropValue.Count + num; j++)
			{
				if (m_subsystemProjectiles.FireProjectile(blockDropValue.Value, position, (0f - m_speed) * vector, Vector3.Zero, null) == null)
				{
					m_subsystemPickables.AddPickable(blockDropValue.Value, 1, position, 1f * vector, null);
				}
			}
		}
		worldItem.ToRemove = true;
	}
}
