using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW;

public class EmptySpaceDeviceBehavior : EntityDeviceBehavior<ComponentEmptySpace>
{
	public SubsystemTerrain m_subsystemTerrain;

	public EmptySpaceDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		if (BlockEntityUtils.GetBlockEntity(componentMiner.m_subsystemTerrain, raycastResult.CellFace.Point, out ComponentBlockEntity blockEntity))
		{
			ComponentEmptySpace componentEmptySpace = blockEntity.Entity.FindComponent<ComponentEmptySpace>(throwOnError: true);
			raycastResult.CellFace.Point = componentEmptySpace.m_root;
			raycastResult.Value = m_subsystemTerrain.Terrain.GetCellValue(componentEmptySpace.m_root.X, componentEmptySpace.m_root.Y, componentEmptySpace.m_root.Z);
			if (componentEmptySpace.m_root != Point3.Zero)
			{
				return Project.FindSubsystem<SubsystemDeviceBlockBehavior>(throwOnError: true).OnInteract(raycastResult, componentMiner);
			}
		}
		return false;
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, new Point3(x, y, z), out ComponentBlockEntity blockEntity))
		{
			Point3 root = blockEntity.Entity.FindComponent<ComponentEmptySpace>(throwOnError: true).m_root;
			if (root != Point3.Zero)
			{
				m_subsystemTerrain.DestroyCell(0, root.X, root.Y, root.Z, 0, noDrop: false, noParticleSystem: false);
			}
		}
		base.OnBlockRemoved(value, newValue, x, y, z);
	}

	public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
	{
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, cellFace.Point, out ComponentBlockEntity blockEntity))
		{
			ComponentEmptySpace componentEmptySpace = blockEntity.Entity.FindComponent<ComponentEmptySpace>(throwOnError: true);
			cellFace.Point = componentEmptySpace.m_root;
			if (componentEmptySpace.m_root != Point3.Zero)
			{
				Project.FindSubsystem<SubsystemDeviceBlockBehavior>(throwOnError: true).OnHitByProjectile(cellFace, worldItem);
			}
		}
	}
}
