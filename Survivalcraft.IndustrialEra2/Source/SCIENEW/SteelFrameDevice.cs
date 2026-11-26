using System.Collections.Generic;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class SteelFrameDevice : SixDirectionModelDevice, IVoltWireDevice, ILadderDevice
{
	public Materials m_material;

	public float m_UL;

	public float m_PL;

	public string? m_containsWireID;

	public SteelFrameDevice(Materials wireMaterial, float ul, float pl, string? containsWireID = null)
		: base(GetCraftingId(wireMaterial, containsWireID), "Models/SteelFrame", "SteelFrame", faceTransparent: true)
	{
		m_material = wireMaterial;
		m_UL = ul;
		m_PL = pl;
		m_containsWireID = containsWireID;
	}

	public static string GetCraftingId(Materials material, string? containsWireID = null)
	{
		if (material == Materials.Plastic && containsWireID == null)
		{
			return "NoneSteelFrameDevice";
		}
		return material.ToString() + "SteelFrameDevice";
	}

	public static string GetID(Materials material)
	{
		return GetCraftingId(material);
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public float GetPL(int value)
	{
		return m_PL;
	}

	public float GetUL(int value)
	{
		return m_UL;
	}

	public Materials GetMaterial()
	{
		return m_material;
	}

	public override string GetCategory(int value)
	{
		return "Construction";
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		base.GenerateTerrainVertices(generator, geometry, value, x, y, z);
		if (m_containsWireID != null)
		{
			WireDrawingManager.GenerateWireVertices(generator, geometry, value, x, y, z, m_material);
		}
	}

	public override int GetMaxStacking(int value)
	{
		return 40;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new SteelFrameDeviceBehavior(project),
			new VoltWireDeviceBehavior(project)
		};
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 107;
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		dropValues.Add(new BlockDropValue
		{
			Value = DevicesBlockManager.GetDeviceValue(GetCraftingId(Materials.Plastic), 0),
			Count = 1
		});
		if (m_containsWireID != null)
		{
			dropValues.Add(new BlockDropValue
			{
				Value = DevicesBlockManager.GetDeviceValue(m_containsWireID, 0),
				Count = 1
			});
		}
	}

	public override bool IsDisplay()
	{
		return m_containsWireID == null;
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 1.2f;
	}
}
