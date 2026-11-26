using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public abstract class DefaultDevice : DeviceBlock
{
	public string DisplayName = string.Empty;

	public string Id = string.Empty;

	public string Description = string.Empty;

	public DefaultDevice(string id)
	{
		Id = id;
		DisplayName = LanguageControl.Get("Devices", "Names", GetCraftingId());
		Description = LanguageControl.Get("Devices", "Descriptions", GetCraftingId());
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return DisplayName;
	}

	public override string GetCraftingId()
	{
		return Id;
	}

	public override string GetDescription(int value)
	{
		return Description;
	}

	public override string GetCategory(int value)
	{
		return "Devices";
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateCubeVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], value, x, y, z, Color.White, geometry.GetGeometry(IndustrialModLoader.IETexture).OpaqueSubsetsByFace);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawCubeBlock(primitivesRenderer, value, new Vector3(size), ref matrix, color, color, environmentData, IndustrialModLoader.IETexture);
	}

	public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		Color white = Color.White;
		return new BlockDebrisParticleSystem(subsystemTerrain, position, 1f, 1f, white, GetFaceTextureSlot(0, value), IndustrialModLoader.IETexture);
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		dropValues.Add(new BlockDropValue
		{
			Value = DevicesBlockManager.GetDeviceValue(Id, 0),
			Count = 1
		});
	}
}
