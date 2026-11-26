using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class GalvanicCellDevice : FourDirectionModelDevice, IVoltDevice
{
	public GalvanicCellDevice()
		: base("GalvanicCellDevice", "Models/IEBattery", "IEBattery", faceTransparent: true)
	{
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public HashSet<int> GetFaceMask(int value)
	{
		return new HashSet<int> { 0, 1, 2, 3, 4, 5 };
	}

	public VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		return new GalvanicCellVoltElement(subsystemVoltNet, subsystemTerrain, position, blockValue);
	}

	public float GetStandardPL(int value)
	{
		return 1f;
	}

	public float GetStandardUL(int value)
	{
		return 1f;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[2]
		{
			new VoltDeviceBehavior(project),
			new GalvanicCellDeviceBehavior(project)
		};
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		BlockDropValue blockDropValue = new BlockDropValue();
		blockDropValue.Value = ItemsBlockManager.GetItemID("CopperPlate");
		blockDropValue.Count = 1;
		BlockDropValue item = blockDropValue;
		for (int i = 0; i < 4; i++)
		{
			dropValues.Add(item);
		}
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 221;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		base.GenerateTerrainVertices(generator, geometry, value, x, y, z);
		WireDrawingManager.GenerateWireVertices(generator, geometry, value, x, y, z, Materials.None);
	}
}
