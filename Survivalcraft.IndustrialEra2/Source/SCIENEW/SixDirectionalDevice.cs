using Game;

namespace SCIENEW;

public abstract class SixDirectionalDevice : DefaultDevice, IRotatableDevice
{
	protected int FrontSlot;

	protected int BackSlot;

	protected int SideSlot;

	public SixDirectionalDevice(string id, int frontSlot, int backSlot = 107, int sideSlot = 107)
		: base(id)
	{
		FrontSlot = frontSlot;
		BackSlot = backSlot;
		SideSlot = sideSlot;
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXYZ(componentMiner));
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return false;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		int direction = DevicesUtils.GetSixFaceDirection(value);
		if (face == direction)
		{
			return FrontSlot;
		}
		if (face == CellFace.OppositeFace(direction))
		{
			return BackSlot;
		}
		return SideSlot;
	}

	public int GetNextDirection(int value, bool reverse = false)
	{
		return DevicesBlockManager.GetDeviceValue(Id, (DevicesUtils.GetSixFaceDirection(value) + (reverse ? 5 : 1)) % 6);
	}
}
