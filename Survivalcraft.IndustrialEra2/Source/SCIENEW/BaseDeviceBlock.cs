using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;

namespace SCIENEW;

public class BaseDeviceBlock : Block, IElectricElementBlock, ICrafter
{
	public static int Index = 503;

	public override bool IsIndexDynamic => false;

	public override void Initialize()
	{
		base.Initialize();
		DefaultEmittedLightAmount = 15;
		LightAttenuation = 1;
		Behaviors = "ElectricBlockBehavior";
		HasCollisionBehavior = true;
		GenerateFacesForSameNeighbors = true;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		DevicesBlockManager.GetDevice(value).GenerateTerrainVertices(generator, geometry, value, x, y, z);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		DevicesBlockManager.GetDevice(value).DrawBlock(primitivesRenderer, value, color, size, ref matrix, environmentData);
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return DevicesBlockManager.GetDevice(value).GetDisplayName(subsystemTerrain, value);
	}

	public override string GetCategory(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetCategory(value);
	}

	public override int GetDisplayOrder(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetDisplayOrder(value);
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return DevicesBlockManager.GetDevice(value).GetIconBlockOffset(value, environmentData);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return DevicesBlockManager.GetDevice(value).GetIconViewOffset(value, environmentData);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return DevicesBlockManager.GetDevice(value).GetIconViewScale(value, environmentData);
	}

	public override float GetFirstPersonScale(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetFirstPersonScale(value);
	}

	public override Vector3 GetFirstPersonOffset(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetFirstPersonOffset(value);
	}

	public override Vector3 GetFirstPersonRotation(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetFirstPersonRotation(value);
	}

	public override float GetInHandScale(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetInHandScale(value);
	}

	public override Vector3 GetInHandOffset(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetInHandOffset(value);
	}

	public override Vector3 GetInHandRotation(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetInHandRotation(value);
	}

	public override string GetCraftingId(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetCraftingId();
	}

	public override IEnumerable<int> GetCreativeValues()
	{
		List<int> list = new List<int>();
		int num = 0;
		foreach (KeyValuePair<string, int[]> item in DevicesBlockManager.IdMap)
		{
			if (DevicesBlockManager.Devices[num++].IsDisplay())
			{
				list.Add(item.Value[0]);
			}
		}
		return list;
	}

	public override bool IsCollidable_(int value)
	{
		return DevicesBlockManager.GetDevice(value).IsCollidable_(value);
	}

	public override bool IsPlaceable_(int value)
	{
		return DevicesBlockManager.GetDevice(value).IsPlaceable_(value);
	}

	public override bool GetIsDiggingTransparent(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetIsDiggingTransparent(value);
	}

	public override bool IsPlacementTransparent_(int value)
	{
		return DevicesBlockManager.GetDevice(value).IsPlacementTransparent_(value);
	}

	public override bool IsInteractive(SubsystemTerrain subsystemTerrain, int value)
	{
		return DevicesBlockManager.GetDevice(value).IsInteractive(subsystemTerrain, value);
	}

	public override bool IsEditable_(int value)
	{
		return DevicesBlockManager.GetDevice(value).IsEditable_(value);
	}

	public override bool IsNonDuplicable_(int value)
	{
		return DevicesBlockManager.GetDevice(value).IsNonDuplicable_(value);
	}

	public override bool IsGatherable_(int value)
	{
		return DevicesBlockManager.GetDevice(value).IsGatherable_(value);
	}

	public override bool HasCollisionBehavior_(int value)
	{
		return DevicesBlockManager.GetDevice(value).HasCollisionBehavior_(value);
	}

	public override bool IsFluidBlocker_(int value)
	{
		return DevicesBlockManager.GetDevice(value).IsFluidBlocker_(value);
	}

	public override bool IsTransparent_(int value)
	{
		return DevicesBlockManager.GetDevice(value).IsTransparent_(value);
	}

	public override int GetShadowStrength(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetShadowStrength(value);
	}

	public override int GetEmittedLightAmount(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetEmittedLightAmount(value);
	}

	public override float GetObjectShadowStrength(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetObjectShadowStrength(value);
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		DevicesBlockManager.GetDevice(oldValue).GetDropValues(subsystemTerrain, oldValue, newValue, toolLevel, dropValues, out showDebris);
	}

	public override float GetRequiredToolLevel(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetRequiredToolLevel(value);
	}

	public override int GetMaxStacking(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetMaxStacking(value);
	}

	public override float GetDensity(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetDensity(value);
	}

	public override float GetFuelHeatLevel(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetFuelHeatLevel(value);
	}

	public override float GetFuelFireDuration(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetFuelFireDuration(value);
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return DevicesBlockManager.GetDevice(value).GetSoundMaterialName(subsystemTerrain, value);
	}

	public override float GetShovelPower(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetShovelPower(value);
	}

	public override float GetQuarryPower(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetQuarryPower(value);
	}

	public override float GetHackPower(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetHackPower(value);
	}

	public override float GetMeleePower(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetMeleePower(value);
	}

	public override float GetMeleeHitProbability(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetMeleeHitProbability(value);
	}

	public override float GetProjectilePower(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetProjectilePower(value);
	}

	public override int GetToolLevel(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetToolLevel(value);
	}

	public override int GetPlayerLevelRequired(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetPlayerLevelRequired(value);
	}

	public override int GetDurability(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetDurability(value);
	}

	public override bool IsAimable_(int value)
	{
		return DevicesBlockManager.GetDevice(value).IsAimable_(value);
	}

	public override bool IsStickable_(int value)
	{
		return DevicesBlockManager.GetDevice(value).IsStickable_(value);
	}

	public override bool GetAlignToVelocity(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetAlignToVelocity(value);
	}

	public override float GetProjectileSpeed(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetProjectileSpeed(value);
	}

	public override float GetProjectileDamping(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetProjectileDamping(value);
	}

	public override float GetProjectileStickProbability(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetProjectileStickProbability(value);
	}

	public override float GetHeat(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetHeat(value);
	}

	public override float GetFireDuration(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetFireDuration(value);
	}

	public override float GetExplosionResilience(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetExplosionResilience(value);
	}

	public override float GetExplosionPressure(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetExplosionPressure(value);
	}

	public override bool GetExplosionIncendiary(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetExplosionIncendiary(value);
	}

	public override BlockDigMethod GetBlockDigMethod(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetBlockDigMethod(value);
	}

	public override float GetDigResilience(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetDigResilience(value);
	}

	public override float GetProjectileResilience(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetProjectileResilience(value);
	}

	public override float GetNutritionalValue(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetNutritionalValue(value);
	}

	public override float GetSicknessProbability(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetSicknessProbability(value);
	}

	public override FoodType GetFoodType(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetFoodType(value);
	}

	public override int GetRotPeriod(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetRotPeriod(value);
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return DevicesBlockManager.GetDevice(value).GetFaceTextureSlot(face, value);
	}

	public override string GetDescription(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetDescription(value);
	}

	public override int GetDamageDestructionValue(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetDamageDestructionValue(value);
	}

	public override bool Eat(ComponentVitalStats vitalStats, int value)
	{
		return DevicesBlockManager.GetDevice(value).Eat(vitalStats, value);
	}

	public override bool CanWear(int value)
	{
		return DevicesBlockManager.GetDevice(value).CanWear(value);
	}

	public override int GetDamage(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetDamage(value);
	}

	public override int SetDamage(int value, int damage)
	{
		return DevicesBlockManager.GetDevice(value).SetDamage(value, damage);
	}

	public override bool ShouldAvoid(int value)
	{
		return DevicesBlockManager.GetDevice(value).ShouldAvoid(value);
	}

	public override ClothingData GetClothingData(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetClothingData(value);
	}

	public override bool IsHeatBlocker(int value)
	{
		return DevicesBlockManager.GetDevice(value).IsHeatBlocker(value);
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		return DevicesBlockManager.GetDevice(value).GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
	}

	public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		return DevicesBlockManager.GetDevice(value).CreateDebrisParticleSystem(subsystemTerrain, position, value, strength);
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return DevicesBlockManager.GetDevice(value).GetCustomCollisionBoxes(terrain, value);
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return DevicesBlockManager.GetDevice(value).IsFaceTransparent(subsystemTerrain, face, value);
	}

	public override int GetTextureSlotCount(int value)
	{
		return DevicesBlockManager.GetDevice(value).GetTextureSlotCount(value);
	}

	public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue, int x, int y, int z)
	{
		return DevicesBlockManager.GetDevice(value).ShouldGenerateFace(subsystemTerrain, face, value, neighborValue);
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		if (!(DevicesBlockManager.GetDevice(value) is IElectricElementDevice electricElementDevice))
		{
			return null;
		}
		return electricElementDevice.CreateElectricElement(subsystemElectricity, value, x, y, z);
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		if (!(DevicesBlockManager.GetDevice(value) is IElectricElementDevice electricElementDevice))
		{
			return null;
		}
		return electricElementDevice.GetConnectorType(terrain, value, face, connectorFace, x, y, z);
	}

	public int GetConnectionMask(int value)
	{
		if (!(DevicesBlockManager.GetDevice(value) is IElectricElementDevice electricElementDevice))
		{
			return 0;
		}
		return electricElementDevice.GetConnectionMask(value);
	}

	public virtual IRecipe? GetDynamicRecipe(IRecipe actual, Project project)
	{
		DeviceBlock[] devices = DevicesBlockManager.Devices;
		for (int i = 0; i < devices.Length; i++)
		{
			IRecipe dynamicRecipe = devices[i].GetDynamicRecipe(actual, project);
			if (dynamicRecipe != null)
			{
				return dynamicRecipe;
			}
		}
		return null;
	}

	public bool IsCrafter(int blockValue, IRecipe recipe)
	{
		return DevicesBlockManager.GetDevice(blockValue).IsCrafter(blockValue, recipe);
	}
}
