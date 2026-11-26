using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class FillerDevice : FourDirectionalDevice, IEntityDevice<ComponentFiller>, IVoltDevice, IElectricElementDevice
{
	public FillerDevice()
		: base("FillerDevice", 220, 99, 99, 99, 99)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "Filler";
	}

	public Widget GetWidget(IInventory inventory, ComponentFiller component)
	{
		return new FillerWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new FillerDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new FillerVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 1f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new CrafterPopResultsElectricElement(subsystemElectricity, new Point3(x, y, z), GetDirection(value));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return recipe is FillingRecipe;
	}
}
