using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class ChemistryFluid : Fluid, IPipeableFluid, IScoopableFluid, IFuelFluid
{
	public string name;

	public Color color;

	public BiDictionary<int, int> m_containers;

	public FluidType m_type;

	public float fuelSpeed;

	public float fuelLevel;

	public bool useLangFileName;

	public ChemistryFluid(string name2, Color color2, FluidType type, BiDictionary<int, int> containers, float fuelLevel = 0f, float fuelSpeed = 0f, bool useLangFileName = false)
	{
		name = name2;
		color = color2;
		m_containers = containers;
		m_type = type;
		this.fuelLevel = fuelLevel;
		this.fuelSpeed = fuelSpeed;
		this.useLangFileName = useLangFileName;
	}

	public override string GetDisplayName(int value)
	{
		return GetLocalizedFluidText("Names", value);
	}

	public override string GetCraftingID(int value)
	{
		return "C:" + name;
	}

	public override string GetDescription(int value)
	{
		return GetLocalizedFluidText("Descriptions", value);
	}

	private string GetLocalizedFluidText(string category, int value)
	{
		string craftingID = GetCraftingID(value);
		string text = LanguageControl.Get("Fluids", category, craftingID);
		if (!string.IsNullOrEmpty(text) && text != craftingID)
		{
			return text;
		}
		return name;
	}

	public override int GetCongenericFluidCount()
	{
		return 1;
	}

	public override Color GetColor(int value)
	{
		return color;
	}

	public bool IsScoopable(int container, out int bucketIndex)
	{
		if (m_containers.ContainsKey(container))
		{
			bucketIndex = m_containers.GetByFirst(container);
			return true;
		}
		bucketIndex = 0;
		return false;
	}

	public BiDictionary<int, int> AcquireContainer(int value)
	{
		return m_containers;
	}

	public override FluidType GetFluidType(int value)
	{
		return m_type;
	}

	public override Texture2D GetTexture(int value)
	{
		if (m_type != FluidType.Gas)
		{
			return base.GetTexture(value);
		}
		return FluidsManager.GasFluidTexture;
	}

	public override int GetTextureSubdivision(int value)
	{
		if (m_type != FluidType.Gas)
		{
			return base.GetTextureSubdivision(value);
		}
		return 1;
	}

	public float GetFuelSpeed(int value)
	{
		return fuelSpeed;
	}

	public float GetFuelLevel(int value)
	{
		return fuelLevel;
	}
}
