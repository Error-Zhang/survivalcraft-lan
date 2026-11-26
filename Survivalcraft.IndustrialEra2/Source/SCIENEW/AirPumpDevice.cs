using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.Graphics;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class AirPumpDevice : ModelDevice, IEntityDevice<ComponentAirPump>, IVoltDevice
{
	public BlockMesh m_fanMesh = new BlockMesh();

	public BlockMesh m_placeFanMesh = new BlockMesh();

	public AirPumpDevice()
		: base("AirPumpDevice", "Models/AirPump", "AirPump", IndustrialModLoader.IETexture, Color.White)
	{
		m_fanMesh.AppendMesh("Models/AirPump", "Fan", Matrix.CreateTranslation(0f, -0.5f, 0f), Matrix.Identity, Color.White);
		m_placeFanMesh.AppendMesh("Models/AirPump", "Fan", Matrix.CreateTranslation(0.5f, 0f, 0.5f), Matrix.Identity, Color.White);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, m_texture, color, size, ref matrix, environmentData);
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_fanMesh, m_texture, color, size, ref matrix, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_blockMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetOpaque);
		if (DevicesUtils.GetInnerData(value) == 0)
		{
			generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_placeFanMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetAlphaTest);
		}
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 99;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public string GetEntityName(int value)
	{
		return "AirPump";
	}

	public Widget GetWidget(IInventory inventory, ComponentAirPump component)
	{
		throw new NotImplementedException();
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new AirPumpVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 3f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new AirPumpDeviceBehavior(project),
			new VoltDeviceBehavior(project)
		};
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}
}
