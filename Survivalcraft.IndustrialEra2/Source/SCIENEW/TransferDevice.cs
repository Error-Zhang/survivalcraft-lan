using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class TransferDevice : SixDirectionModelDevice, IElectricElementDevice
{
	public TransferDevice()
		: base("TransferDevice", "Models/Transfer", "Transfer", faceTransparent: false, useStandaloneMesh: false)
	{
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXYZ(componentMiner, isOpposite: true));
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new TransferDeviceBehavior(project)
		};
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new TransferElectricElement(subsystemElectricity, new Point3(x, y, z));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + -2;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 170;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}
}
