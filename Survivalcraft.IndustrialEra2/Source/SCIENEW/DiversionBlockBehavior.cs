using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class DiversionBlockBehavior : DeviceBehavior
{
	public SubsystemPickables m_subsystemPickables;

	public SubsystemProjectiles m_subsystemProjectiles;

	public SubsystemTerrain m_subsystemTerrain;

	public DiversionBlockBehavior(Project project)
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
		m_subsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
		Vector3 vector = CellFace.FaceToVector3(DevicesUtils.GetSixFaceDirection(m_subsystemTerrain.Terrain.GetCellValueFast(cellFace.X, cellFace.Y, cellFace.Z)));
		Vector3 velocity = worldItem.Velocity;
		int num = 1;
		if (worldItem is Pickable pickable)
		{
			num = pickable.Count;
		}
		while (num > 0)
		{
			if (m_subsystemProjectiles.FireProjectile(worldItem.Value, new Vector3(cellFace.X, cellFace.Y, cellFace.Z) + new Vector3(0.5f) + 0.75f * vector, vector * velocity.Length(), Vector3.Zero, null) == null)
			{
				m_subsystemPickables.AddPickable(worldItem.Value, num, new Vector3(cellFace.X, cellFace.Y, cellFace.Z) + new Vector3(0.5f) + 0.75f * vector, 1f * vector, null);
				break;
			}
			num--;
		}
		worldItem.ToRemove = true;
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
