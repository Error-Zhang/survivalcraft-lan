using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW.VoltNet;

public static class CondenserPortUtils
{
	public static CondenserPortRole? GetPortRole(DeviceBlock? device)
	{
		if (device is CondenserInputPortDevice)
		{
			return CondenserPortRole.Input;
		}
		if (device is CondenserOutputPortDevice)
		{
			return CondenserPortRole.Output;
		}
		return null;
	}

	public static CondenserPortRole? GetPortRole(int blockValue)
	{
		if (BlockInterfaceResolver.ResolveFromValue<CondenserInputPortDevice>(blockValue) != null)
		{
			return CondenserPortRole.Input;
		}
		if (BlockInterfaceResolver.ResolveFromValue<CondenserOutputPortDevice>(blockValue) != null)
		{
			return CondenserPortRole.Output;
		}
		return null;
	}

	public static Point3 GetHostPosition(int portValue, Point3 portPosition)
	{
		int innerData = DevicesUtils.GetInnerData(portValue);
		return portPosition + CellFace.FaceToPoint3(CellFace.OppositeFace(innerData));
	}

	public static void TryRebindHostAt(Project project, Point3 hostPosition)
	{
		if (BlockEntityUtils.GetBlockEntity(project.FindSubsystem<SubsystemTerrain>(throwOnError: true), hostPosition, out ComponentBlockEntity blockEntity))
		{
			blockEntity.Entity.FindComponent<ComponentCondenser>(throwOnError: false)?.RebindPorts();
		}
	}

	public static void RebindHostFromPort(Project project, Point3 portPosition, int portValue)
	{
		TryRebindHostAt(project, GetHostPosition(portValue, portPosition));
	}

	public static void MaybeRebindCondenserHost(Project project, int x, int y, int z)
	{
		SubsystemTerrain subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		Point3 point = new Point3(x, y, z);
		int cellValueFast = subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
		if (GetPortRole(cellValueFast).HasValue)
		{
			RebindHostFromPort(project, point, cellValueFast);
		}
		else
		{
			TryRebindHostAt(project, point);
		}
	}

	public static bool TryGetHostVoltRatings(SubsystemTerrain subsystemTerrain, Point3 hostPosition, out float standardPL, out float standardUL)
	{
		standardPL = 0f;
		standardUL = 0f;
		int cellValue = subsystemTerrain.Terrain.GetCellValue(hostPosition.X, hostPosition.Y, hostPosition.Z);
		IVoltDevice voltDevice = BlockInterfaceResolver.ResolveFromValue<IVoltDevice>(cellValue);
		if (voltDevice == null)
		{
			return false;
		}
		standardPL = voltDevice.GetStandardPL(cellValue);
		standardUL = voltDevice.GetStandardUL(cellValue);
		return true;
	}

	public static bool TryGetHostCondenser(SubsystemTerrain subsystemTerrain, Point3 portPosition, int portValue, CondenserPortRole expectedRole, out ComponentCondenser? condenser)
	{
		condenser = null;
		if (GetPortRole(portValue) != expectedRole)
		{
			return false;
		}
		Point3 hostPosition = GetHostPosition(portValue, portPosition);
		if (!BlockEntityUtils.GetBlockEntity(subsystemTerrain, hostPosition, out ComponentBlockEntity blockEntity))
		{
			return false;
		}
		ComponentCondenser componentCondenser = blockEntity.Entity.FindComponent<ComponentCondenser>(throwOnError: false);
		if (componentCondenser == null)
		{
			return false;
		}
		int innerData = DevicesUtils.GetInnerData(portValue);
		if (!componentCondenser.HasPortOnFace(innerData))
		{
			return false;
		}
		condenser = componentCondenser;
		return true;
	}
}
