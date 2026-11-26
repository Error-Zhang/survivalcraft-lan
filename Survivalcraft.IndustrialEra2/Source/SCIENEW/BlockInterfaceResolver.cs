using System;
using System.Collections.Generic;
using Engine;
using Game;
using SCIENEW.VoltNet;

namespace SCIENEW;

public static class BlockInterfaceResolver
{
	public static bool HasBehaviorTag(string? behaviors, string tag)
	{
		if (string.IsNullOrWhiteSpace(behaviors))
		{
			return false;
		}
		string[] array = behaviors.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (string.Equals(array[i].Trim(), tag, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	public static object GetInterfaceProviderByValue(int blockValue)
	{
		int num = Terrain.ExtractContents(blockValue);
		Block block = BlocksManager.Blocks[num];
		if (block is BaseDeviceBlock)
		{
			return DevicesBlockManager.GetDevice(blockValue);
		}
		return block;
	}

	public static object? GetInterfaceProviderByPoint(SubsystemTerrain terrain, Point3 point, out int blockValue)
	{
		blockValue = terrain.Terrain.GetCellValueFast(point.X, point.Y, point.Z);
		return GetInterfaceProviderByValue(blockValue);
	}

	public static T? ResolveAt<T>(SubsystemTerrain terrain, Point3 point, out int blockValue) where T : class
	{
		return GetInterfaceProviderByPoint(terrain, point, out blockValue) as T;
	}

	public static T? ResolveFromValue<T>(int blockValue) where T : class
	{
		return GetInterfaceProviderByValue(blockValue) as T;
	}

	public static bool HasFluidConnectorOnFace(Terrain terrain, int blockValue, int connectorFace, int cellX, int cellY, int cellZ)
	{
		IFluidTransferrerDevice fluidTransferrerDevice = ResolveFromValue<IFluidTransferrerDevice>(blockValue);
		if (fluidTransferrerDevice == null)
		{
			return false;
		}
		return fluidTransferrerDevice.GetConnector(terrain, blockValue, connectorFace, cellX, cellY, cellZ) != null;
	}

	public static HashSet<int> GetVoltConnectionFaceMask(int blockValue)
	{
		object interfaceProviderByValue = GetInterfaceProviderByValue(blockValue);
		if (interfaceProviderByValue is IVoltDevice voltDevice)
		{
			return voltDevice.GetFaceMask(blockValue);
		}
		if (interfaceProviderByValue is IVoltWireDevice voltWireDevice)
		{
			return voltWireDevice.GetFaceMask(blockValue);
		}
		return new HashSet<int>();
	}
}
