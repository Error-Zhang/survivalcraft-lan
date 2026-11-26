using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class CondenserDevice : FourDirectionalDevice, IEntityDevice<ComponentCondenser>, IVoltDevice, IElectricElementDevice
{
	public CondenserDevice()
		: base("CondenserDevice", 117, 117, 117, 99, 99)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "Condenser";
	}

	public Widget GetWidget(IInventory inventory, ComponentCondenser component)
	{
		return new CondenserWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new CondenserDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new CondenserVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 10f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new CondenserElectricElement(subsystemElectricity, new Point3(x, y, z));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}
}
