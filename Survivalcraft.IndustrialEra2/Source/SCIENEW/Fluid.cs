using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public abstract class Fluid
{
	public string DisplayName;

	public string Description;

	public Fluid()
	{
		DisplayName = LanguageControl.Get("Fluids", "Names", GetCraftingID(0));
		Description = LanguageControl.Get("Fluids", "Descriptions", GetCraftingID(0));
	}

	public virtual string GetDisplayName(int value)
	{
		return DisplayName;
	}

	public virtual string GetCraftingID(int value)
	{
		return GetType().Name;
	}

	public virtual string GetDescription(int value)
	{
		return Description;
	}

	public virtual Texture2D GetTexture(int value)
	{
		return FluidsManager.NormalFluidTexture;
	}

	public virtual int GetTextureSubdivision(int value)
	{
		return 6;
	}

	public virtual int GetTextureSlot(int value)
	{
		return 0;
	}

	public virtual Color GetColor(int value)
	{
		return Color.White;
	}

	public virtual int GetCongenericFluidCount()
	{
		return 1;
	}

	public virtual void Emit(Project project, Vector3 position, Vector3 velocity)
	{
	}

	public virtual bool IsGlow(int value)
	{
		return false;
	}

	public virtual FluidType GetFluidType(int value)
	{
		return FluidType.Liquid;
	}
}
