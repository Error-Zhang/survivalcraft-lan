using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class SeparatorDevice : FourDirectionalDevice, IVoltDevice, IEntityDevice<ComponentSeparator>
{
	public SeparatorDevice()
		: base("SeparatorDevice", 240, 99, 99, 99, 99)
	{
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new SeparatorVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 1f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new SeparatorDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "Separator";
	}

	public Widget GetWidget(IInventory inventory, ComponentSeparator component)
	{
		return new SeparatorWidget(inventory, component);
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		if (recipe is SiftingRecipe siftingRecipe)
		{
			return siftingRecipe.ProcessType == "Sifting";
		}
		return false;
	}
}
