using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CastingCheckValveDevice : SixDirectionModelDevice, IEntityDevice<ComponentCastingCheckValve>, IFluidTransferrerDevice
{
	public BlockMesh m_displayBlockMesh = new BlockMesh();

	public CastingCheckValveDevice()
		: base("CastingCheckValveDevice", "Models/CastingCheckValve", "CastingCheckValve", faceTransparent: true)
	{
		m_displayBlockMesh.AppendBlockMesh(m_blockMeshesByFace[1]);
		m_displayBlockMesh.TransformPositions(Matrix.CreateTranslation(-0.5f, -0.5f, -0.5f));
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 3;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new CastingCheckValveDeviceBehavior(project),
			new FluidTransferrerDeviceBehavior(project)
		};
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_displayBlockMesh, IndustrialModLoader.IETexture, color, size, ref matrix, environmentData);
	}

	public FluidTransferrer? CreateFluidTransferrer(SubsystemFluidTransfer subsystemFluidTransfer, SubsystemTerrain subsystemTerrain, int value, Point3 point)
	{
		return new CastingCheckValveTransferrer(point, subsystemTerrain);
	}

	public FluidConnector? GetConnector(Terrain terrain, int value, int face, int x, int y, int z)
	{
		if (face == SixDirectionModelDevice.GetDirection_Static(value))
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Output, 1f);
		}
		if (CellFace.OppositeFace(face) == SixDirectionModelDevice.GetDirection_Static(value))
		{
			return new FluidConnector(new CellFace(x, y, z, face), FluidConnectorType.Input, 1f);
		}
		return null;
	}

	public string GetEntityName(int value)
	{
		return "CastingCheckValve";
	}

	public Widget GetWidget(IInventory inventory, ComponentCastingCheckValve component)
	{
		return new CastingCheckValveWidget(inventory, component);
	}

	public override int GetMaxStacking(int value)
	{
		return 40;
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Stone";
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 4;
	}
}
