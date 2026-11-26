using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using RecipaediaEX;
using SCIENEW.RecipaediaEX;

namespace SCIENEW;

public abstract class NormalBlock : IIEBlock, IDynamicRecipeProvider
{
	public virtual void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateCubeVertices(BlocksManager.Blocks[BaseNormalBlock.Index], value, x, y, z, GetColor(value), geometry.GetGeometry(GetTexture(value)).OpaqueSubsetsByFace);
	}

	public virtual void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawCubeBlock(primitivesRenderer, value, new Vector3(size), ref matrix, GetColor(value) * color, GetColor(value) * color, environmentData, GetTexture(value));
	}

	public virtual string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return string.Empty;
	}

	public virtual string GetCategory(int value)
	{
		return "Construction";
	}

	public virtual int GetDisplayOrder(int value)
	{
		return 318;
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

	public virtual float GetFirstPersonScale(int value)
	{
		return 0.4f;
	}

	public virtual Vector3 GetFirstPersonOffset(int value)
	{
		return new Vector3(0.5f, -0.5f, -0.6f);
	}

	public virtual Vector3 GetFirstPersonRotation(int value)
	{
		return new Vector3(0f, 40f, 0f);
	}

	public virtual float GetInHandScale(int value)
	{
		return 0.3f;
	}

	public virtual Vector3 GetInHandOffset(int value)
	{
		return new Vector3(0f, 0.12f, 0f);
	}

	public virtual Vector3 GetInHandRotation(int value)
	{
		return new Vector3(0f, 0f, 45f);
	}

	public virtual string GetCraftingId()
	{
		return GetType().Name;
	}

	public virtual bool IsCollidable_(int value)
	{
		return true;
	}

	public virtual bool IsPlaceable_(int value)
	{
		return true;
	}

	public virtual bool GetIsDiggingTransparent(int value)
	{
		return false;
	}

	public virtual bool IsPlacementTransparent_(int value)
	{
		return false;
	}

	public virtual bool IsInteractive(SubsystemTerrain subsystemTerrain, int value)
	{
		return false;
	}

	public virtual bool IsEditable_(int value)
	{
		return false;
	}

	public virtual bool IsNonDuplicable_(int value)
	{
		return false;
	}

	public virtual bool IsGatherable_(int value)
	{
		return false;
	}

	public virtual bool HasCollisionBehavior_(int value)
	{
		return false;
	}

	public virtual bool IsFluidBlocker_(int value)
	{
		return true;
	}

	public virtual bool IsTransparent_(int value)
	{
		return false;
	}

	public virtual int GetShadowStrength(int value)
	{
		return 15;
	}

	public virtual int GetEmittedLightAmount(int value)
	{
		return 0;
	}

	public virtual float GetObjectShadowStrength(int value)
	{
		return 1f;
	}

	public virtual void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		dropValues.Add(new BlockDropValue
		{
			Value = Terrain.ReplaceLight(oldValue, 0),
			Count = 1
		});
	}

	public virtual float GetRequiredToolLevel(int value)
	{
		return 0f;
	}

	public virtual int GetMaxStacking(int value)
	{
		return 40;
	}

	public virtual float GetDensity(int value)
	{
		return 10f;
	}

	public virtual float GetFuelHeatLevel(int value)
	{
		return 0f;
	}

	public virtual float GetFuelFireDuration(int value)
	{
		return 0f;
	}

	public virtual string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Metal";
	}

	public virtual float GetShovelPower(int value)
	{
		return 1f;
	}

	public virtual float GetQuarryPower(int value)
	{
		return 1f;
	}

	public virtual float GetHackPower(int value)
	{
		return 1f;
	}

	public virtual float GetMeleePower(int value)
	{
		return 1f;
	}

	public virtual float GetMeleeHitProbability(int value)
	{
		return 0.66f;
	}

	public virtual float GetProjectilePower(int value)
	{
		return 2f;
	}

	public virtual int GetToolLevel(int value)
	{
		return 1;
	}

	public virtual int GetPlayerLevelRequired(int value)
	{
		return 0;
	}

	public virtual int GetDurability(int value)
	{
		return -1;
	}

	public virtual bool IsAimable_(int value)
	{
		return false;
	}

	public virtual bool IsStickable_(int value)
	{
		return false;
	}

	public virtual bool GetAlignToVelocity(int value)
	{
		return false;
	}

	public virtual float GetProjectileSpeed(int value)
	{
		return 0f;
	}

	public virtual float GetProjectileDamping(int value)
	{
		return 1f;
	}

	public virtual float GetProjectileStickProbability(int value)
	{
		return 0f;
	}

	public virtual float GetHeat(int value)
	{
		return 0f;
	}

	public virtual float GetFireDuration(int value)
	{
		return 0f;
	}

	public virtual float GetExplosionResilience(int value)
	{
		return 2000f;
	}

	public virtual float GetExplosionPressure(int value)
	{
		return 0f;
	}

	public virtual bool GetExplosionIncendiary(int value)
	{
		return false;
	}

	public virtual BlockDigMethod GetBlockDigMethod(int value)
	{
		return BlockDigMethod.Quarry;
	}

	public virtual float GetDigResilience(int value)
	{
		return 20f;
	}

	public virtual float GetProjectileResilience(int value)
	{
		return 1f;
	}

	public virtual float GetNutritionalValue(int value)
	{
		return 0f;
	}

	public virtual float GetSicknessProbability(int value)
	{
		return 0f;
	}

	public virtual FoodType GetFoodType(int value)
	{
		return FoodType.None;
	}

	public virtual int GetRotPeriod(int value)
	{
		return 0;
	}

	public virtual int GetFaceTextureSlot(int face, int value)
	{
		return 0;
	}

	public virtual string GetDescription(int value)
	{
		return string.Empty;
	}

	public virtual int GetDamageDestructionValue(int value)
	{
		return 1;
	}

	public virtual bool Eat(ComponentVitalStats vitalStats, int value)
	{
		return false;
	}

	public virtual bool CanWear(int value)
	{
		return false;
	}

	public virtual int GetDamage(int value)
	{
		return 1;
	}

	public virtual int SetDamage(int value, int damage)
	{
		return 1;
	}

	public virtual bool ShouldAvoid(int value)
	{
		return false;
	}

	public virtual ClothingData GetClothingData(int value)
	{
		return null;
	}

	public virtual bool IsHeatBlocker(int value)
	{
		return false;
	}

	public virtual BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.Value = value;
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public virtual BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		return null;
	}

	public virtual BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return Block.m_defaultCollisionBoxes;
	}

	public virtual bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return false;
	}

	public virtual int GetTextureSlotCount(int value)
	{
		return 16;
	}

	public virtual bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue)
	{
		int num = Terrain.ExtractContents(neighborValue);
		return BlocksManager.Blocks[num].IsFaceTransparent(subsystemTerrain, CellFace.OppositeFace(face), neighborValue);
	}

	public virtual Texture2D GetTexture(int value)
	{
		return IndustrialModLoader.BaseTexture;
	}

	public virtual Color GetColor(int value)
	{
		return Color.White;
	}

	public virtual NormalBlockBehavior[] GetBehaviors(Project project)
	{
		return Array.Empty<NormalBlockBehavior>();
	}

	public virtual IRecipe? GetDynamicRecipe(IRecipe actual, Project project)
	{
		return null;
	}
}
