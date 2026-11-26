using Engine;
using Game;

namespace SCIENEW;

public abstract class FourDirectionalDevice : DefaultDevice, IRotatableDevice
{
	public int FrontSlot;

	public int SideSlot;

	public int BackSlot;

	public int UpSlot;

	public int DownSlot;

	public bool Rotate;

	public FourDirectionalDevice(string id, int frontSlot, int backSlot = 107, int sideSlot = 107, int upSlot = 107, int downSlot = 107, bool rotate = false)
		: base(id)
	{
		FrontSlot = frontSlot;
		SideSlot = sideSlot;
		BackSlot = backSlot;
		UpSlot = upSlot;
		DownSlot = downSlot;
		Rotate = rotate;
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.CellFace = raycastResult.CellFace;
		result.Value = DevicesBlockManager.GetDeviceValue(Id, DevicesUtils.GetDirectionXZ(componentMiner));
		return result;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return false;
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
				return FrontSlot;
			}
			if (face == CellFace.OppositeFace(GetDirection(value)))
			{
				return BackSlot;
			}
			return SideSlot;
		}
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		int direction = GetDirection(value);
		if (Rotate && (direction == 1 || direction == 3))
		{
			generator.GenerateCubeVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], value, x, y, z, 0, 1, 0, Color.White, geometry.GetGeometry(IndustrialModLoader.IETexture).OpaqueSubsetsByFace);
		}
		else
		{
			generator.GenerateCubeVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], value, x, y, z, Color.White, geometry.GetGeometry(IndustrialModLoader.IETexture).OpaqueSubsetsByFace);
		}
	}

	protected virtual int GetDirection(int value)
	{
		return GetDirection_Static(value);
	}

	public static int GetDirection_Static(int value)
	{
		return DevicesUtils.GetInnerData(value) & 3;
	}

	public int GetNextDirection(int value, bool reverse = false)
	{
		return DevicesBlockManager.GetDeviceValue(Id, (GetDirection_Static(value) + 1) % 4);
	}
}
