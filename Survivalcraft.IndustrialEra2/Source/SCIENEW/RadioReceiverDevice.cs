using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class RadioReceiverDevice : SixDirectionModelDevice, IElectricElementDevice, IVoltDevice
{
	public RadioReceiverDevice()
		: base("RadioReceiverDevice", "Models/RadioReceiver", "RadioReceiver", faceTransparent: true)
	{
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.CellFace = raycastResult.CellFace;
		result.Value = DevicesBlockManager.GetDeviceValue(Id, raycastResult.CellFace.Face);
		return result;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		base.GenerateTerrainVertices(generator, geometry, value, x, y, z);
		generator.GenerateWireVertices(value, x, y, z, GetDirection(value), 0.8f, Vector2.Zero, geometry.SubsetOpaque);
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 99;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new RadioReceiverDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new RadioReceiverElectricElement(subsystemElectricity, new CellFace(x, y, z, GetDirection(value)));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		if (GetDirection(value) == face)
		{
			return ElectricConnectorType.Output;
		}
		return null;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public HashSet<int> GetFaceMask(int value)
	{
		HashSet<int> hashSet = new HashSet<int>();
		hashSet.Add(0);
		hashSet.Add(1);
		hashSet.Add(2);
		hashSet.Add(3);
		hashSet.Add(4);
		hashSet.Add(5);
		hashSet.Remove(GetDirection(value));
		return hashSet;
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new RadioVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 1f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}
}
