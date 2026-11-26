using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class SifterDevice : FourDirectionModelDevice, IEntityDevice<ComponentSifter>, IElectricElementDevice
{
	public SifterDevice()
		: base("SifterDevice", "Models/Sifter", "Sifter")
	{
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Wood";
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 186;
	}

	public string GetEntityName(int value)
	{
		return "Sifter";
	}

	public Widget GetWidget(IInventory inventory, ComponentSifter component)
	{
		return null;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new SifterDeviceBehavior(project)
		};
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) - 1;
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
		if (recipe is SiftingRecipe siftingRecipe)
		{
			return siftingRecipe.ProcessType == "Sifting";
		}
		return false;
	}
}
