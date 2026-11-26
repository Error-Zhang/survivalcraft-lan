using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;

namespace SCIENEW;

public class BaseNormalBlock : CubeBlock
{
	public static int Index = 501;

	public override bool IsIndexDynamic => false;

	public override void Initialize()
	{
		base.Initialize();
		DigMethod = BlockDigMethod.Quarry;
		DigResilience = 1f;
		ExplosionResilience = 1f;
		GenerateFacesForSameNeighbors = true;
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		NormalBlockManager.GetNormalBlock(value).GenerateTerrainVertices(generator, geometry, value, x, y, z);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		NormalBlockManager.GetNormalBlock(value).DrawBlock(primitivesRenderer, value, color, size, ref matrix, environmentData);
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetDisplayName(subsystemTerrain, value);
	}

	public override string GetCategory(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetCategory(value);
	}

	public override int GetDisplayOrder(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetDisplayOrder(value);
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return NormalBlockManager.GetNormalBlock(value).GetIconBlockOffset(value, environmentData);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return NormalBlockManager.GetNormalBlock(value).GetIconViewOffset(value, environmentData);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return NormalBlockManager.GetNormalBlock(value).GetIconViewScale(value, environmentData);
	}

	public override float GetFirstPersonScale(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetFirstPersonScale(value);
	}

	public override Vector3 GetFirstPersonOffset(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetFirstPersonOffset(value);
	}

	public override Vector3 GetFirstPersonRotation(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetFirstPersonRotation(value);
	}

	public override float GetInHandScale(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetInHandScale(value);
	}

	public override Vector3 GetInHandOffset(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetInHandOffset(value);
	}

	public override Vector3 GetInHandRotation(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetInHandRotation(value);
	}

	public override string GetCraftingId(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetCraftingId();
	}

	public override IEnumerable<int> GetCreativeValues()
	{
		int[] array = new int[Enumerable.Count(NormalBlockManager.IdMap)];
		int num = 0;
		foreach (KeyValuePair<string, int> item in NormalBlockManager.IdMap)
		{
			array[num++] = item.Value;
		}
		return array;
	}

	public override bool IsCollidable_(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsCollidable_(value);
	}

	public override bool IsPlaceable_(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsPlaceable_(value);
	}

	public override bool GetIsDiggingTransparent(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetIsDiggingTransparent(value);
	}

	public override bool IsPlacementTransparent_(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsPlacementTransparent_(value);
	}

	public override bool IsInteractive(SubsystemTerrain subsystemTerrain, int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsInteractive(subsystemTerrain, value);
	}

	public override bool IsEditable_(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsEditable_(value);
	}

	public override bool IsNonDuplicable_(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsNonDuplicable_(value);
	}

	public override bool IsGatherable_(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsGatherable_(value);
	}

	public override bool HasCollisionBehavior_(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).HasCollisionBehavior_(value);
	}

	public override bool IsFluidBlocker_(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsFluidBlocker_(value);
	}

	public override bool IsTransparent_(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsTransparent_(value);
	}

	public override int GetShadowStrength(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetShadowStrength(value);
	}

	public override int GetEmittedLightAmount(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetEmittedLightAmount(value);
	}

	public override float GetObjectShadowStrength(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetObjectShadowStrength(value);
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		NormalBlockManager.GetNormalBlock(oldValue).GetDropValues(subsystemTerrain, oldValue, newValue, toolLevel, dropValues, out showDebris);
	}

	public override float GetRequiredToolLevel(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetRequiredToolLevel(value);
	}

	public override int GetMaxStacking(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetMaxStacking(value);
	}

	public override float GetDensity(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetDensity(value);
	}

	public override float GetFuelHeatLevel(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetFuelHeatLevel(value);
	}

	public override float GetFuelFireDuration(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetFuelFireDuration(value);
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetSoundMaterialName(subsystemTerrain, value);
	}

	public override float GetShovelPower(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetShovelPower(value);
	}

	public override float GetQuarryPower(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetQuarryPower(value);
	}

	public override float GetHackPower(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetHackPower(value);
	}

	public override float GetMeleePower(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetMeleePower(value);
	}

	public override float GetMeleeHitProbability(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetMeleeHitProbability(value);
	}

	public override float GetProjectilePower(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetProjectilePower(value);
	}

	public override int GetToolLevel(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetToolLevel(value);
	}

	public override int GetPlayerLevelRequired(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetPlayerLevelRequired(value);
	}

	public override int GetDurability(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetDurability(value);
	}

	public override bool IsAimable_(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsAimable_(value);
	}

	public override bool IsStickable_(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsStickable_(value);
	}

	public override bool GetAlignToVelocity(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetAlignToVelocity(value);
	}

	public override float GetProjectileSpeed(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetProjectileSpeed(value);
	}

	public override float GetProjectileDamping(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetProjectileDamping(value);
	}

	public override float GetProjectileStickProbability(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetProjectileStickProbability(value);
	}

	public override float GetHeat(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetHeat(value);
	}

	public override float GetFireDuration(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetFireDuration(value);
	}

	public override float GetExplosionResilience(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetExplosionResilience(value);
	}

	public override float GetExplosionPressure(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetExplosionPressure(value);
	}

	public override bool GetExplosionIncendiary(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetExplosionIncendiary(value);
	}

	public override BlockDigMethod GetBlockDigMethod(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetBlockDigMethod(value);
	}

	public override float GetDigResilience(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetDigResilience(value);
	}

	public override float GetProjectileResilience(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetProjectileResilience(value);
	}

	public override float GetNutritionalValue(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetNutritionalValue(value);
	}

	public override float GetSicknessProbability(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetSicknessProbability(value);
	}

	public override FoodType GetFoodType(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetFoodType(value);
	}

	public override int GetRotPeriod(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetRotPeriod(value);
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetFaceTextureSlot(face, value);
	}

	public override string GetDescription(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetDescription(value);
	}

	public override int GetDamageDestructionValue(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetDamageDestructionValue(value);
	}

	public override bool Eat(ComponentVitalStats vitalStats, int value)
	{
		return NormalBlockManager.GetNormalBlock(value).Eat(vitalStats, value);
	}

	public override bool CanWear(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).CanWear(value);
	}

	public override int GetDamage(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetDamage(value);
	}

	public override int SetDamage(int value, int damage)
	{
		return NormalBlockManager.GetNormalBlock(value).SetDamage(value, damage);
	}

	public override bool ShouldAvoid(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).ShouldAvoid(value);
	}

	public override ClothingData GetClothingData(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetClothingData(value);
	}

	public override bool IsHeatBlocker(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsHeatBlocker(value);
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		return NormalBlockManager.GetNormalBlock(value).GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
	}

	public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		return NormalBlockManager.GetNormalBlock(value).CreateDebrisParticleSystem(subsystemTerrain, position, value, strength);
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetCustomCollisionBoxes(terrain, value);
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return NormalBlockManager.GetNormalBlock(value).IsFaceTransparent(subsystemTerrain, face, value);
	}

	public override int GetTextureSlotCount(int value)
	{
		return NormalBlockManager.GetNormalBlock(value).GetTextureSlotCount(value);
	}

	public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue, int x, int y, int z)
	{
		return NormalBlockManager.GetNormalBlock(value).ShouldGenerateFace(subsystemTerrain, face, value, neighborValue);
	}

	public virtual IRecipe? GetDynamicRecipe(IRecipe actual, Project project)
	{
		NormalBlock[] blocks = NormalBlockManager.Blocks;
		for (int i = 0; i < blocks.Length; i++)
		{
			IRecipe dynamicRecipe = blocks[i].GetDynamicRecipe(actual, project);
			if (dynamicRecipe != null)
			{
				return dynamicRecipe;
			}
		}
		return null;
	}
}
