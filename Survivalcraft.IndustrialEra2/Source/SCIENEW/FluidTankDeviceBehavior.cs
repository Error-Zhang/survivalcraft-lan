using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW;

public class FluidTankDeviceBehavior : EntityDeviceBehavior<ComponentFluidTank>
{
	public SubsystemTerrain m_subsystemTerrain;

	public FluidTankDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		if (IndustrialEraLanAdapter.IsRemote(Project))
		{
			return;
		}
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(x, z) != null)
		{
			int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y - 1, z);
			if (BlocksManager.Blocks[Terrain.ExtractContents(cellValueFast)].IsTransparent_(cellValueFast) && (!(BlocksManager.Blocks[Terrain.ExtractContents(cellValueFast)] is BaseDeviceBlock) || !(DevicesBlockManager.GetDevice(cellValueFast) is FluidTankDevice)))
			{
				m_subsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
			}
		}
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		if (IndustrialEraLanAdapter.IsRemote(Project))
		{
			return;
		}
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y - 1, z);
		int num = Terrain.ExtractContents(cellValueFast);
		if (BlocksManager.Blocks[num] is BaseDeviceBlock && DevicesBlockManager.GetDevice(cellValueFast) is FluidTankDevice)
		{
			ComponentBlockEntity blockEntity;
			for (Point3 position = new Point3(x, y, z); !BlockEntityUtils.GetBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), position, out blockEntity) || blockEntity.Entity.FindComponent<ComponentFluidTank>() == null; position -= Point3.UnitY)
			{
			}
			blockEntity?.Entity.FindComponent<ComponentFluidTank>(throwOnError: true).UpdateVolume();
		}
		else
		{
			base.OnBlockAdded(value, oldValue, x, y, z);
		}
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		if (IndustrialEraLanAdapter.IsRemote(Project))
		{
			return;
		}
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y - 1, z);
		int num = Terrain.ExtractContents(cellValueFast);
		if (BlocksManager.Blocks[num] is BaseDeviceBlock && DevicesBlockManager.GetDevice(cellValueFast) is FluidTankDevice)
		{
			ComponentBlockEntity blockEntity;
			for (Point3 position = new Point3(x, y, z); !BlockEntityUtils.GetBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), position, out blockEntity) || blockEntity.Entity.FindComponent<ComponentFluidTank>() == null; position -= Point3.UnitY)
			{
			}
			blockEntity?.Entity.FindComponent<ComponentFluidTank>(throwOnError: true).UpdateVolume();
		}
		else
		{
			base.OnBlockRemoved(value, newValue, x, y, z);
		}
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		if (IndustrialEraLanAdapter.IsRemote(Project))
		{
			return;
		}
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y - 1, z);
		int num = Terrain.ExtractContents(cellValueFast);
		if (BlocksManager.Blocks[num] is BaseDeviceBlock && DevicesBlockManager.GetDevice(cellValueFast) is FluidTankDevice)
		{
			ComponentBlockEntity blockEntity;
			for (Point3 position = new Point3(x, y, z); !BlockEntityUtils.GetBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), position, out blockEntity) || blockEntity.Entity.FindComponent<ComponentFluidTank>() == null; position -= Point3.UnitY)
			{
			}
			blockEntity?.Entity.FindComponent<ComponentFluidTank>(throwOnError: true).UpdateVolume();
		}
		else
		{
			base.OnBlockModified(value, oldValue, x, y, z);
		}
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		int num = Terrain.ExtractContents(componentMiner.ActiveBlockValue);
		if (BlocksManager.Blocks[num] is BaseDeviceBlock && DevicesBlockManager.GetDevice(componentMiner.ActiveBlockValue) is FluidTankDevice)
		{
			return false;
		}
		ComponentBlockEntity blockEntity;
		while (!BlockEntityUtils.GetBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), raycastResult.CellFace.Point, out blockEntity))
		{
			raycastResult.CellFace.Point -= Point3.UnitY;
		}
		return base.OnInteract(raycastResult, componentMiner);
	}
}
