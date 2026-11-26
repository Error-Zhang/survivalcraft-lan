using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using RecipaediaEX.Implementation;

namespace SCIENEW;

public class IronFurnaceDevice : FourDirectionModelDevice, IEntityDevice<ComponentIronFurnace>, IElectricElementDevice
{
	public IronFurnaceDevice()
		: base("IronFurnaceDevice", "Models/IronFurnace", "Furnace")
	{
	}

	public override int GetEmittedLightAmount(int value)
	{
		if (!IsLight(value))
		{
			return 0;
		}
		return 13;
	}

	public override float GetHeat(int value)
	{
		if (!IsLight(value))
		{
			return 0f;
		}
		return 0.66f;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return IsLight(value);
	}

	public override bool IsTransparent_(int value)
	{
		return IsLight(value);
	}

	public override int GetShadowStrength(int value)
	{
		if (!IsLight(value))
		{
			return base.GetShadowStrength(value);
		}
		return -99;
	}

	protected static bool IsLight(int value)
	{
		return DevicesUtils.GetInnerData(value) >> 2 == 1;
	}

	public string GetEntityName(int value)
	{
		return "IronFurnace";
	}

	public Widget GetWidget(IInventory inventory, ComponentIronFurnace component)
	{
		return new IronFurnaceWidget(inventory, component);
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new IronFurnaceDeviceBehavior(project)
		};
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
		return recipe.GetType().IsAssignableTo(typeof(OriginalSmeltingRecipe));
	}
}
