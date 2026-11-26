using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX;

namespace SCIENEW;

public class DamageableItemBlock : IIEBlock, IDynamicRecipeProvider
{
	public virtual void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
	}

	public virtual void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
	}

	public virtual string GetCategory(int value)
	{
		return "Items";
	}

	public virtual void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubset geometry, int value, int x, int y, int z)
	{
	}

	public virtual bool Eat(ComponentVitalStats vitalStats, int value)
	{
		return false;
	}

	public virtual int GetDamage(int value)
	{
		return DamageableItemManager.GetDamage(value);
	}

	public virtual bool CanWear(int value)
	{
		return false;
	}

	public virtual string GetDescription(int value)
	{
		return GetType().Name;
	}

	public virtual int GetDurability(int value)
	{
		return -1;
	}

	public virtual float GetDensity(int value)
	{
		return 3f;
	}

	public virtual float GetHeat(int value)
	{
		return 0f;
	}

	public virtual bool IsAimable_(int value)
	{
		return false;
	}

	public virtual bool IsCollidable_(int value)
	{
		return true;
	}

	public virtual bool IsEditable_(int value)
	{
		return false;
	}

	public virtual bool IsInteractive(SubsystemTerrain subsystemTerrain, int value)
	{
		return false;
	}

	public virtual bool IsGatherable_(int value)
	{
		return false;
	}

	public virtual bool IsPlaceable_(int value)
	{
		return false;
	}

	public virtual int SetDamage(int value, int damage)
	{
		return DamageableItemManager.SetDamage(value, damage);
	}

	public virtual string GetCraftingId()
	{
		return GetType().Name;
	}

	public virtual bool IsStickable_(int value)
	{
		return false;
	}

	public virtual bool IsTransparent_(int value)
	{
		return false;
	}

	public virtual bool ShouldAvoid(int value)
	{
		return false;
	}

	public virtual ClothingData GetClothingData(int value)
	{
		return null;
	}

	public virtual BlockPlacementData GetDigValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, int toolValue, TerrainRaycastResult raycastResult)
	{
		return default;
	}

	public virtual string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return GetType().Name;
	}

	public virtual float GetDigResilience(int value)
	{
		return 0f;
	}

	public virtual void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = false;
	}

	public virtual bool GetExplosionIncendiary(int value)
	{
		return false;
	}

	public virtual float GetExplosionPressure(int value)
	{
		return 0f;
	}

	public virtual int GetDisplayOrder(int value)
	{
		return 1000;
	}

	public virtual int GetMaxStacking(int value)
	{
		return 1;
	}

	public virtual float GetMeleePower(int value)
	{
		return 1f;
	}

	public virtual float GetExplosionResilience(int value)
	{
		return 0f;
	}

	public virtual float GetFireDuration(int value)
	{
		return 0f;
	}

	public virtual FoodType GetFoodType(int value)
	{
		return FoodType.None;
	}

	public virtual float GetNutritionalValue(int value)
	{
		return 0f;
	}

	public virtual BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		return default;
	}

	public virtual float GetHackPower(int value)
	{
		return 1f;
	}

	public virtual float GetProjectilePower(int value)
	{
		return 2f;
	}

	public virtual float GetProjectileDamping(int value)
	{
		return 1f;
	}

	public virtual float GetProjectileResilience(int value)
	{
		return 0f;
	}

	public virtual float GetProjectileSpeed(int value)
	{
		return 0f;
	}

	public virtual int GetRotPeriod(int value)
	{
		return 0;
	}

	public virtual float GetQuarryPower(int value)
	{
		return 1f;
	}

	public virtual int GetShadowStrength(int value)
	{
		return 0;
	}

	public virtual float GetSicknessProbability(int value)
	{
		return 0f;
	}

	public virtual float GetShovelPower(int value)
	{
		return 1f;
	}

	public virtual int GetToolLevel(int value)
	{
		return 0;
	}

	public virtual bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public virtual bool HasCollisionBehavior_(int value)
	{
		return false;
	}

	public virtual bool IsFluidBlocker_(int value)
	{
		return false;
	}

	public virtual bool IsHeatBlocker(int value)
	{
		return false;
	}

	public virtual bool IsNonDuplicable_(int value)
	{
		return false;
	}

	public virtual bool IsPlacementTransparent_(int value)
	{
		return false;
	}

	public virtual BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		return new BlockDebrisParticleSystem(subsystemTerrain, position, strength, 1f, Color.White, GetFaceTextureSlot(4, value));
	}

	public virtual BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return Block.m_defaultCollisionBoxes;
	}

	public virtual bool GetAlignToVelocity(int value)
	{
		return false;
	}

	public virtual int GetDamageDestructionValue(int value)
	{
		return 0;
	}

	public virtual int GetFaceTextureSlot(int face, int value)
	{
		return 0;
	}

	public virtual BlockDigMethod GetBlockDigMethod(int value)
	{
		return BlockDigMethod.None;
	}

	public virtual BoundingBox[] GetCustomInteractionBoxes(SubsystemTerrain terrain, int value)
	{
		return null;
	}

	public virtual int GetEmittedLightAmount(int value)
	{
		return 0;
	}

	public virtual float GetFuelFireDuration(int value)
	{
		return 0f;
	}

	public virtual Texture2D GetDiggingCrackingTexture(ComponentMiner miner, float digProgress, int value, Texture2D[] defaultCrackTextures)
	{
		return IndustrialModLoader.BaseTexture;
	}

	public virtual Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return Vector3.Zero;
	}

	public virtual Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return Vector3.One;
	}

	public virtual float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 1f;
	}

	public virtual Vector3 GetFirstPersonOffset(int value)
	{
		return new Vector3(0.5f, -0.5f, -0.6f);
	}

	public virtual Vector3 GetFirstPersonRotation(int value)
	{
		return new Vector3(0f, 40f, 0f);
	}

	public virtual float GetFirstPersonScale(int value)
	{
		return 0.5f;
	}

	public virtual float GetFuelHeatLevel(int value)
	{
		return 0f;
	}

	public virtual float GetMeleeHitProbability(int value)
	{
		return 0f;
	}

	public virtual Vector3 GetInHandOffset(int value)
	{
		return new Vector3(0f, 0f, 0f);
	}

	public virtual Vector3 GetInHandRotation(int value)
	{
		return new Vector3(60f, 0f, 0f);
	}

	public virtual float GetInHandScale(int value)
	{
		return 0.3f;
	}

	public virtual bool GetIsDiggingTransparent(int value)
	{
		return false;
	}

	public virtual float GetObjectShadowStrength(int value)
	{
		return 0f;
	}

	public virtual int GetPlayerLevelRequired(int value)
	{
		return 0;
	}

	public virtual float GetProjectileStickProbability(int value)
	{
		return 0f;
	}

	public virtual float GetRequiredToolLevel(int value)
	{
		return 0f;
	}

	public virtual string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return string.Empty;
	}

	public virtual int GetTextureSlotCount(int value)
	{
		return 16;
	}

	public virtual bool IsSwapAnimationNeeded(int oldValue, int newValue)
	{
		return true;
	}

	public virtual DamageableItemBehavior[] GetBehaviors(Project project)
	{
		return Array.Empty<DamageableItemBehavior>();
	}

	public virtual bool IsDisplay()
	{
		return true;
	}

	public virtual IRecipe? GetDynamicRecipe(IRecipe actual, Project project)
	{
		return null;
	}
}
