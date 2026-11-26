using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;

namespace SCIENEW;

public class BaseItemBlock : Block
{
	public static int Index = 500;

	public override bool IsIndexDynamic => false;

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		ItemsBlockManager.GetItem(value).GenerateTerrainVertices(generator, geometry, value, x, y, z);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		ItemsBlockManager.GetItem(value).DrawBlock(primitivesRenderer, value, color, size, ref matrix, environmentData);
	}

	public override string GetCategory(int value)
	{
		return ItemsBlockManager.GetItem(value).GetCategory(value);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubset geometry, int value, int x, int y, int z)
	{
		ItemsBlockManager.GetItem(value).GenerateTerrainVertices(generator, geometry, value, x, y, z);
	}

	public override bool Eat(ComponentVitalStats vitalStats, int value)
	{
		return ItemsBlockManager.GetItem(value).Eat(vitalStats, value);
	}

	public override int GetDamage(int value)
	{
		return ItemsBlockManager.GetItem(value).GetDamage(value);
	}

	public override bool CanWear(int value)
	{
		return ItemsBlockManager.GetItem(value).CanWear(value);
	}

	public override string GetDescription(int value)
	{
		return ItemsBlockManager.GetItem(value).GetDescription(value);
	}

	public override int GetDurability(int value)
	{
		return ItemsBlockManager.GetItem(value).GetDurability(value);
	}

	public override float GetDensity(int value)
	{
		return ItemsBlockManager.GetItem(value).GetDensity(value);
	}

	public override float GetHeat(int value)
	{
		return ItemsBlockManager.GetItem(value).GetHeat(value);
	}

	public override bool IsAimable_(int value)
	{
		return ItemsBlockManager.GetItem(value).IsAimable_(value);
	}

	public override bool IsCollidable_(int value)
	{
		return ItemsBlockManager.GetItem(value).IsCollidable_(value);
	}

	public override bool IsEditable_(int value)
	{
		return ItemsBlockManager.GetItem(value).IsEditable_(value);
	}

	public override bool IsInteractive(SubsystemTerrain subsystemTerrain, int value)
	{
		return ItemsBlockManager.GetItem(value).IsInteractive(subsystemTerrain, value);
	}

	public override bool IsGatherable_(int value)
	{
		return ItemsBlockManager.GetItem(value).IsGatherable_(value);
	}

	public override bool IsPlaceable_(int value)
	{
		return ItemsBlockManager.GetItem(value).IsPlaceable_(value);
	}

	public override int SetDamage(int value, int damage)
	{
		return ItemsBlockManager.GetItem(value).SetDamage(value, damage);
	}

	public override string GetCraftingId(int value)
	{
		return ItemsBlockManager.GetItem(value).GetCraftingId();
	}

	public override bool IsStickable_(int value)
	{
		return ItemsBlockManager.GetItem(value).IsStickable_(value);
	}

	public override bool IsTransparent_(int value)
	{
		return ItemsBlockManager.GetItem(value).IsTransparent_(value);
	}

	public override bool ShouldAvoid(int value)
	{
		return ItemsBlockManager.GetItem(value).ShouldAvoid(value);
	}

	public override ClothingData GetClothingData(int value)
	{
		return ItemsBlockManager.GetItem(value).GetClothingData(value);
	}

	public override IEnumerable<int> GetCreativeValues()
	{
		List<int> list = new List<int>();
		int num = 0;
		foreach (KeyValuePair<string, int> item in ItemsBlockManager.IdMap)
		{
			if (ItemsBlockManager.Items[num++].IsDisplay())
			{
				list.Add(item.Value);
			}
		}
		return list;
	}

	public override BlockPlacementData GetDigValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, int toolValue, TerrainRaycastResult raycastResult)
	{
		return ItemsBlockManager.GetItem(value).GetDigValue(subsystemTerrain, componentMiner, value, toolValue, raycastResult);
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return ItemsBlockManager.GetItem(value).GetDisplayName(subsystemTerrain, value);
	}

	public override float GetDigResilience(int value)
	{
		return ItemsBlockManager.GetItem(value).GetDigResilience(value);
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		ItemsBlockManager.GetItem(oldValue).GetDropValues(subsystemTerrain, oldValue, newValue, toolLevel, dropValues, out showDebris);
	}

	public override bool GetExplosionIncendiary(int value)
	{
		return ItemsBlockManager.GetItem(value).GetExplosionIncendiary(value);
	}

	public override float GetExplosionPressure(int value)
	{
		return ItemsBlockManager.GetItem(value).GetExplosionPressure(value);
	}

	public override int GetDisplayOrder(int value)
	{
		return ItemsBlockManager.GetItem(value).GetDisplayOrder(value);
	}

	public override int GetMaxStacking(int value)
	{
		return ItemsBlockManager.GetItem(value).GetMaxStacking(value);
	}

	public override float GetMeleePower(int value)
	{
		return ItemsBlockManager.GetItem(value).GetMeleePower(value);
	}

	public override float GetExplosionResilience(int value)
	{
		return ItemsBlockManager.GetItem(value).GetExplosionResilience(value);
	}

	public override float GetFireDuration(int value)
	{
		return ItemsBlockManager.GetItem(value).GetFireDuration(value);
	}

	public override FoodType GetFoodType(int value)
	{
		return ItemsBlockManager.GetItem(value).GetFoodType(value);
	}

	public override float GetNutritionalValue(int value)
	{
		return ItemsBlockManager.GetItem(value).GetNutritionalValue(value);
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		return ItemsBlockManager.GetItem(value).GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
	}

	public override float GetHackPower(int value)
	{
		return ItemsBlockManager.GetItem(value).GetHackPower(value);
	}

	public override float GetProjectilePower(int value)
	{
		return ItemsBlockManager.GetItem(value).GetProjectilePower(value);
	}

	public override float GetProjectileDamping(int value)
	{
		return ItemsBlockManager.GetItem(value).GetProjectileDamping(value);
	}

	public override float GetProjectileResilience(int value)
	{
		return ItemsBlockManager.GetItem(value).GetProjectileResilience(value);
	}

	public override float GetProjectileSpeed(int value)
	{
		return ItemsBlockManager.GetItem(value).GetProjectileSpeed(value);
	}

	public override int GetRotPeriod(int value)
	{
		return ItemsBlockManager.GetItem(value).GetRotPeriod(value);
	}

	public override float GetQuarryPower(int value)
	{
		return ItemsBlockManager.GetItem(value).GetQuarryPower(value);
	}

	public override int GetShadowStrength(int value)
	{
		return ItemsBlockManager.GetItem(value).GetShadowStrength(value);
	}

	public override float GetSicknessProbability(int value)
	{
		return ItemsBlockManager.GetItem(value).GetSicknessProbability(value);
	}

	public override float GetShovelPower(int value)
	{
		return ItemsBlockManager.GetItem(value).GetShovelPower(value);
	}

	public override int GetToolLevel(int value)
	{
		return ItemsBlockManager.GetItem(value).GetToolLevel(value);
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return ItemsBlockManager.GetItem(value).IsFaceTransparent(subsystemTerrain, face, value);
	}

	public override bool HasCollisionBehavior_(int value)
	{
		return ItemsBlockManager.GetItem(value).HasCollisionBehavior_(value);
	}

	public override bool IsFluidBlocker_(int value)
	{
		return ItemsBlockManager.GetItem(value).IsFluidBlocker_(value);
	}

	public override bool IsHeatBlocker(int value)
	{
		return ItemsBlockManager.GetItem(value).IsHeatBlocker(value);
	}

	public override bool IsNonDuplicable_(int value)
	{
		return ItemsBlockManager.GetItem(value).IsNonDuplicable_(value);
	}

	public override bool IsPlacementTransparent_(int value)
	{
		return ItemsBlockManager.GetItem(value).IsPlacementTransparent_(value);
	}

	public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		return ItemsBlockManager.GetItem(value).CreateDebrisParticleSystem(subsystemTerrain, position, value, strength);
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return ItemsBlockManager.GetItem(value).GetCustomCollisionBoxes(terrain, value);
	}

	public override bool GetAlignToVelocity(int value)
	{
		return ItemsBlockManager.GetItem(value).GetAlignToVelocity(value);
	}

	public override int GetDamageDestructionValue(int value)
	{
		return ItemsBlockManager.GetItem(value).GetDamageDestructionValue(value);
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return ItemsBlockManager.GetItem(value).GetFaceTextureSlot(face, value);
	}

	public override BlockDigMethod GetBlockDigMethod(int value)
	{
		return ItemsBlockManager.GetItem(value).GetBlockDigMethod(value);
	}

	public override BoundingBox[] GetCustomInteractionBoxes(SubsystemTerrain terrain, int value)
	{
		return ItemsBlockManager.GetItem(value).GetCustomInteractionBoxes(terrain, value);
	}

	public override int GetEmittedLightAmount(int value)
	{
		return ItemsBlockManager.GetItem(value).GetEmittedLightAmount(value);
	}

	public override float GetFuelFireDuration(int value)
	{
		return ItemsBlockManager.GetItem(value).GetFuelFireDuration(value);
	}

	public override Texture2D GetDiggingCrackingTexture(ComponentMiner miner, float digProgress, int value, Texture2D[] defaultCrackTextures)
	{
		return ItemsBlockManager.GetItem(value).GetDiggingCrackingTexture(miner, digProgress, value, defaultCrackTextures);
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return ItemsBlockManager.GetItem(value).GetIconBlockOffset(value, environmentData);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return ItemsBlockManager.GetItem(value).GetIconViewOffset(value, environmentData);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return ItemsBlockManager.GetItem(value).GetIconViewScale(value, environmentData);
	}

	public override Vector3 GetFirstPersonOffset(int value)
	{
		return ItemsBlockManager.GetItem(value).GetFirstPersonOffset(value);
	}

	public override Vector3 GetFirstPersonRotation(int value)
	{
		return ItemsBlockManager.GetItem(value).GetFirstPersonRotation(value);
	}

	public override float GetFirstPersonScale(int value)
	{
		return ItemsBlockManager.GetItem(value).GetFirstPersonScale(value);
	}

	public override float GetFuelHeatLevel(int value)
	{
		return ItemsBlockManager.GetItem(value).GetFuelHeatLevel(value);
	}

	public override float GetMeleeHitProbability(int value)
	{
		return ItemsBlockManager.GetItem(value).GetMeleeHitProbability(value);
	}

	public override Vector3 GetInHandOffset(int value)
	{
		return ItemsBlockManager.GetItem(value).GetInHandOffset(value);
	}

	public override Vector3 GetInHandRotation(int value)
	{
		return ItemsBlockManager.GetItem(value).GetInHandRotation(value);
	}

	public override float GetInHandScale(int value)
	{
		return ItemsBlockManager.GetItem(value).GetInHandScale(value);
	}

	public override bool GetIsDiggingTransparent(int value)
	{
		return ItemsBlockManager.GetItem(value).GetIsDiggingTransparent(value);
	}

	public override float GetObjectShadowStrength(int value)
	{
		return ItemsBlockManager.GetItem(value).GetObjectShadowStrength(value);
	}

	public override int GetPlayerLevelRequired(int value)
	{
		return ItemsBlockManager.GetItem(value).GetPlayerLevelRequired(value);
	}

	public override float GetProjectileStickProbability(int value)
	{
		return ItemsBlockManager.GetItem(value).GetProjectileStickProbability(value);
	}

	public override float GetRequiredToolLevel(int value)
	{
		return ItemsBlockManager.GetItem(value).GetRequiredToolLevel(value);
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return ItemsBlockManager.GetItem(value).GetSoundMaterialName(subsystemTerrain, value);
	}

	public override int GetTextureSlotCount(int value)
	{
		return ItemsBlockManager.GetItem(value).GetTextureSlotCount(value);
	}

	public override bool IsSwapAnimationNeeded(int oldValue, int newValue)
	{
		return ItemsBlockManager.GetItem(oldValue).IsSwapAnimationNeeded(oldValue, newValue);
	}

	public virtual IRecipe? GetDynamicRecipe(IRecipe actual, Project project)
	{
		ItemBlock[] items = ItemsBlockManager.Items;
		for (int i = 0; i < items.Length; i++)
		{
			IRecipe dynamicRecipe = items[i].GetDynamicRecipe(actual, project);
			if (dynamicRecipe != null)
			{
				return dynamicRecipe;
			}
		}
		return null;
	}
}
