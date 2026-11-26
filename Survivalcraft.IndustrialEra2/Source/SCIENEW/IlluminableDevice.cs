using Game;

namespace SCIENEW;

public abstract class IlluminableDevice : FourDirectionalDevice, IRotatableDevice
{
	public int LightFrontSlot;

	public IlluminableDevice(string id, int frontSlot, int lightFrontSlot, int backSlot = 107, int sideSlot = 159, int upSlot = 107, int downSlot = 107)
		: base(id, frontSlot, backSlot, sideSlot, upSlot, downSlot)
	{
		LightFrontSlot = lightFrontSlot;
	}

	public override int GetEmittedLightAmount(int value)
	{
		if (!IsLight(value))
		{
			return 0;
		}
		return 15;
	}

	public override float GetHeat(int value)
	{
		if (!IsLight(value))
		{
			return 0f;
		}
		return 0.66f;
	}

	protected static bool IsLight(int value)
	{
		return DevicesUtils.GetInnerData(value) >> 2 == 1;
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

	protected override int GetDirection(int value)
	{
		return GetDirection_Static(value);
	}

	public new static int GetDirection_Static(int value)
	{
		return DevicesUtils.GetInnerData(value) & 3;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		switch (face)
		{
		case 4:
			return UpSlot;
		case 5:
			return DownSlot;
		default:
			if (face == GetDirection(value))
			{
				if (!IsLight(value))
				{
					return FrontSlot;
				}
				return LightFrontSlot;
			}
			if (face == CellFace.OppositeFace(GetDirection(value)))
			{
				return BackSlot;
			}
			return SideSlot;
		}
	}

	public new int GetNextDirection(int value, bool reverse = false)
	{
		return DevicesBlockManager.GetDeviceValue(Id, ((GetDirection_Static(value) + 1) % 4) | (IsLight(value) ? 4 : 0));
	}
}
