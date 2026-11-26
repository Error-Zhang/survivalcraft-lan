using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class EmptySpaceDevice : DefaultDevice, IEntityDevice<ComponentEmptySpace>
{
	public EmptySpaceDevice()
		: base("EmptySpaceDevice")
	{
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
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
		showDebris = false;
	}

	public override int GetShadowStrength(int value)
	{
		return -99;
	}

	public override float GetObjectShadowStrength(int value)
	{
		return 0f;
	}

	public string GetEntityName(int value)
	{
		return "EmptySpace";
	}

	public Widget GetWidget(IInventory inventory, ComponentEmptySpace component)
	{
		throw new NotImplementedException();
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new EmptySpaceDeviceBehavior(project)
		};
	}

	public override bool IsDisplay()
	{
		return false;
	}
}
