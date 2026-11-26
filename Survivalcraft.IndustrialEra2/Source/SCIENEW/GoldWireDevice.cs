using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class GoldWireDevice : DefaultDevice, IVoltWireDevice
{
	public BlockMesh m_standaloneBlockMesh;

	public GoldWireDevice()
		: base("GoldWireDevice")
	{
		m_standaloneBlockMesh = WireDrawingManager.GetStandaloneBlockMesh(Materials.Gold);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		WireDrawingManager.GenerateWireVertices(generator, geometry, value, x, y, z, Materials.Gold);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, IndustrialModLoader.IETexture, color, size, ref matrix, environmentData);
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 4;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override float GetObjectShadowStrength(int value)
	{
		return 0f;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 19;
	}

	public override int GetMaxStacking(int value)
	{
		return 40;
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public override bool IsCollidable_(int value)
	{
		return false;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new VoltWireDeviceBehavior(project)
		};
	}

	public float GetPL(int value)
	{
		return IEConstants.VoltNet.GoldWire.PL;
	}

	public float GetUL(int value)
	{
		return IEConstants.VoltNet.GoldWire.UL;
	}

	public Materials GetMaterial()
	{
		return Materials.Gold;
	}
}
