using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX.Recipes;

namespace SCIENEW;

public class HearthFurnaceCastingBasinDevice : FourDirectionModelDevice, IEntityDevice<ComponentHearthFurnaceCastingBasin>, IFluidTransferrerDevice
{
	public HearthFurnaceCastingBasinDevice()
		: base("HearthFurnaceCastingBasinDevice", "Models/CastingBasin", "HearthFurnaceCastingBasin", faceTransparent: true, doubleSided: true)
	{
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 4;
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return new BoundingBox[1]
		{
			new BoundingBox(0f, 0f, 0f, 1f, 0.5f, 1f)
		};
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Stone";
	}

	public string GetEntityName(int value)
	{
		return "HearthFurnaceCastingBasin";
	}

	public Widget GetWidget(IInventory inventory, ComponentHearthFurnaceCastingBasin component)
	{
		return null;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new HearthFurnaceCastingBasinDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new CastingBasinTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		if (face == CellFace.OppositeFace(GetDirection(value)))
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 1f);
		}
		return null;
	}

	public override bool IsCrafter(int blockValue, IRecipe recipe)
	{
		if (recipe is CastingEquationRecipe castingEquationRecipe)
		{
			return castingEquationRecipe.ProcessType == "HearthCasting";
		}
		return false;
	}
}
