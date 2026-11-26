using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class OilTankDevice : ModelDevice
{
	public OilTankDevice()
		: base("OilTankDevice", "Models/OilTank", "OilTank", IndustrialModLoader.IETexture, Color.White)
	{
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return LanguageControl.Get("Devices", "Names", IsFull(value) ? (GetCraftingId() + "_Full") : GetCraftingId());
	}

	public static int SetFull(int value)
	{
		return DevicesUtils.SetInnerData(value, 1);
	}

	public static int SetEmpty(int value)
	{
		return DevicesUtils.SetInnerData(value, 0);
	}

	public static bool IsFull(int value)
	{
		return DevicesUtils.GetInnerData(value) == 1;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		dropValues.Add(new BlockDropValue
		{
			Value = DevicesBlockManager.GetDeviceValue(Id, IsFull(oldValue) ? 1 : 0),
			Count = 1
		});
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 107;
	}

	public override int GetMaxStacking(int value)
	{
		return 40;
	}

	public override string GetCategory(int value)
	{
		return "Tools";
	}

	public override int GetDisplayOrder(int value)
	{
		return 1000;
	}
}
