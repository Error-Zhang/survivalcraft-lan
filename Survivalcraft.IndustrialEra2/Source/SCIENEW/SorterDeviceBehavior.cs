using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SorterDeviceBehavior : EntityDeviceBehavior<ComponentChest>
{
	public SubsystemPickables m_subsystemPickables;

	public SubsystemProjectiles m_subsystemProjectiles;

	public SubsystemTerrain m_subsystemTerrain;

	public SorterDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemPickables = project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_subsystemProjectiles = project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
		m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
	{
		if (worldItem.ToRemove)
		{
			return;
		}
		ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(cellFace.X, cellFace.Y, cellFace.Z);
		if (blockEntity == null || ((worldItem as Pickable)?.Count ?? 1) > 1)
		{
			return;
		}
		Vector3 vector = new Vector3(cellFace.Point) + new Vector3(0.5f);
		ComponentChest componentChest = blockEntity.Entity.FindComponent<ComponentChest>(throwOnError: true);
		if (componentChest.GetSlotValue(0) == worldItem.Value)
		{
			worldItem.ToRemove = true;
			Vector3 vector2 = new Vector3(-1f, 0f, 0f);
			if (m_subsystemProjectiles.FireProjectile(worldItem.Value, vector + 0.75f * vector2, 10f * vector2, Vector3.Zero, null) == null)
			{
				m_subsystemPickables.AddPickable(worldItem.Value, 1, vector + 0.75f * vector2, 1f * vector2, null);
			}
		}
		else if (componentChest.GetSlotValue(1) == worldItem.Value)
		{
			worldItem.ToRemove = true;
			Vector3 vector2 = new Vector3(0f, 0f, 1f);
			if (m_subsystemProjectiles.FireProjectile(worldItem.Value, vector + 0.75f * vector2, 10f * vector2, Vector3.Zero, null) == null)
			{
				m_subsystemPickables.AddPickable(worldItem.Value, 1, vector + 0.75f * vector2, 1f * vector2, null);
			}
		}
		else if (componentChest.GetSlotValue(2) == worldItem.Value)
		{
			worldItem.ToRemove = true;
			Vector3 vector2 = new Vector3(1f, 0f, 0f);
			if (m_subsystemProjectiles.FireProjectile(worldItem.Value, vector + 0.75f * vector2, 10f * vector2, Vector3.Zero, null) == null)
			{
				m_subsystemPickables.AddPickable(worldItem.Value, 1, vector + 0.75f * vector2, 1f * vector2, null);
			}
		}
		else if (componentChest.GetSlotValue(3) == worldItem.Value)
		{
			worldItem.ToRemove = true;
			Vector3 vector2 = new Vector3(0f, 0f, -1f);
			if (m_subsystemProjectiles.FireProjectile(worldItem.Value, vector + 0.75f * vector2, 10f * vector2, Vector3.Zero, null) == null)
			{
				m_subsystemPickables.AddPickable(worldItem.Value, 1, vector + 0.75f * vector2, 1f * vector2, null);
			}
		}
		else
		{
			Vector3 vector2 = -CellFace.FaceToVector3(cellFace.Face);
			worldItem.ToRemove = true;
			if (m_subsystemProjectiles.FireProjectile(worldItem.Value, vector + 0.75f * vector2, 10f * vector2, Vector3.Zero, null) == null)
			{
				m_subsystemPickables.AddPickable(worldItem.Value, 1, vector + 0.75f * vector2, 1f * vector2, null);
			}
		}
	}

	public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody)
	{
		int x = cellFace.X;
		int y = cellFace.Y;
		int z = cellFace.Z;
		if (m_subsystemTerrain.Terrain.GetCellContentsFast(x, y, z) == IceBlock.Index && !(componentBody.Mass < 999f) && !(componentBody.Density > 1f))
		{
			m_subsystemTerrain.DestroyCell(0, x, y, z, WaterBlock.Index, noDrop: false, noParticleSystem: false);
		}
	}
}
