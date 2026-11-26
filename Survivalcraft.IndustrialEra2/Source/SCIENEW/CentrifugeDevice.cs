using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class CentrifugeDevice : FourDirectionalDevice, IVoltDevice, IEntityDevice<ComponentCentrifuge>
{
	public CentrifugeDevice()
		: base("CentrifugeDevice", 240, 128, 128, 128, 128)
	{
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new CentrifugeVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 2f;
	}

	public float GetStandardUL(int value)
	{
		return 2f;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new CentrifugeDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "Centrifuge";
	}

	public Widget GetWidget(IInventory inventory, ComponentCentrifuge component)
	{
		return new CentrifugeWidget(inventory, component);
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		if (recipe is SiftingRecipe siftingRecipe)
		{
			return siftingRecipe.ProcessType == "Centrifuging";
		}
		return false;
	}
}
