using System;
using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SpringPlateDevice : ModelDevice, IEntityDevice<ComponentSpringPlate>
{
	public SpringPlateDevice()
		: base("SpringPlateDevice", "Models/SpringPlate", "SpringPlate", IndustrialModLoader.IETexture, Color.White)
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

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		if (!IsScaling(value))
		{
			base.GenerateTerrainVertices(generator, geometry, value, x, y, z);
		}
	}

	public bool IsScaling(int value)
	{
		return DevicesUtils.GetInnerData(value) == 1;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 107;
	}

	public string GetEntityName(int value)
	{
		return "SpringPlate";
	}

	public Widget GetWidget(IInventory inventory, ComponentSpringPlate component)
	{
		throw new NotImplementedException();
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new SpringPlateDeviceBehavior(project)
		};
	}
}
