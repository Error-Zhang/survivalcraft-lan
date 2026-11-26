using Engine;
using Engine.Graphics;
using GameEntitySystem;

namespace SCIENEW;

public class AirFluid : Fluid
{
	public override Color GetColor(int value)
	{
		return Color.Transparent;
	}

	public override int GetTextureSlot(int value)
	{
		return 0;
	}

	public override void Emit(Project project, Vector3 position, Vector3 velocity)
	{
	}

	public override int GetCongenericFluidCount()
	{
		return 1;
	}

	public override Texture2D GetTexture(int value)
	{
		return IndustrialModLoader.IETexture;
	}

	public override int GetTextureSubdivision(int value)
	{
		return 16;
	}

	public override string GetCraftingID(int value)
	{
		return "AirFluid";
	}

	public override FluidType GetFluidType(int value)
	{
		return FluidType.Gas;
	}
}
