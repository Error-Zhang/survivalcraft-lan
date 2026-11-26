using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW;

public class OresBlock : CubeBlock
{
	public static int Index = 502;

	public readonly Materials[] Mats = new Materials[12]
	{
		Materials.Gold,
		Materials.Silver,
		Materials.Platinum,
		Materials.Lead,
		Materials.Zinc,
		Materials.Stannary,
		Materials.Mercury,
		Materials.Chromium,
		Materials.Titanium,
		Materials.Nickel,
		Materials.Uranium,
		Materials.Phosphorus
	};

	public override string GetCategory(int value)
	{
		return "Minerals";
	}

	public override float GetRequiredToolLevel(int value)
	{
		return 3f;
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		int data = Terrain.ExtractData(oldValue);
		if (toolLevel > 3 && IsExplosion(data) && (new Engine.Random().Int() & 3) == 0)
		{
			dropValues.Add(new BlockDropValue
			{
				Value = oldValue,
				Count = 1
			});
			showDebris = true;
			return;
		}
		if (toolLevel > 2)
		{
			Materials material = GetMaterial(data);
			if (material == Materials.Uranium && IndustrialModLoader.IsAprilFools)
			{
				dropValues.Add(new BlockDropValue
				{
					Value = ItemsBlockManager.GetItemID("AF_What"),
					Count = 5
				});
			}
			if ((new Engine.Random().Int() & 0xF) == 0)
			{
				dropValues.Add(new BlockDropValue
				{
					Value = ItemsBlockManager.GetItemID("ScrapIron"),
					Count = 1
				});
			}
			else
			{
				int num = new Engine.Random().Int(1, 2);
				int itemIdByMaterial = ItemsBlockManager.GetItemIdByMaterial<OreChunkItem>(material);
				for (int i = 0; i < num; i++)
				{
					dropValues.Add(new BlockDropValue
					{
						Value = itemIdByMaterial,
						Count = 1
					});
				}
				foreach (var companionOre in OresDataBase.GetCompanionOres(material))
				{
					if (new Engine.Random().Float() < companionOre.Probability)
					{
						int itemIdByMaterial2 = ItemsBlockManager.GetItemIdByMaterial<OreChunkItem>(companionOre.Ore);
						dropValues.Add(new BlockDropValue
						{
							Value = itemIdByMaterial2,
							Count = 1
						});
					}
				}
			}
			int num2 = new Engine.Random().Int(1, 4);
			for (int j = 0; j < num2; j++)
			{
				dropValues.Add(new BlockDropValue
				{
					Value = ExperienceBlock.Index,
					Count = 1
				});
			}
			showDebris = true;
		}
		showDebris = true;
	}

	public override BlockPlacementData GetDigValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, int toolValue, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.Value = (IsUnstable(Terrain.ExtractData(value)) ? Terrain.ReplaceData(MagmaBlock.Index, FluidBlock.SetIsTop(FluidBlock.SetLevel(0, 4), toolValue == MagmaBucketBlock.Index)) : 0);
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		Materials material = GetMaterial(Terrain.ExtractData(value));
		return Mats.FirstIndex(material) + 244;
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		int data = Terrain.ExtractData(value);
		string text = string.Empty;
		if (IsUnstable(data))
		{
			text += LanguageControl.Get("Ores", "Unstable");
		}
		if (IsExplosion(data))
		{
			text += LanguageControl.Get("Ores", "Explodable");
		}
		Materials material = GetMaterial(data);
		return text + material.ToDisplayName() + LanguageControl.Get("Ores", "Ore");
	}

	public override float GetExplosionPressure(int value)
	{
		if (!IsExplosion(Terrain.ExtractData(value)))
		{
			return base.GetExplosionPressure(value);
		}
		return 10f;
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawCubeBlock(primitivesRenderer, value, new Vector3(size), ref matrix, color, color, environmentData, IndustrialModLoader.IETexture);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateCubeVertices(this, value, x, y, z, Color.White, geometry.GetGeometry(IndustrialModLoader.IETexture).OpaqueSubsetsByFace);
	}

	public override IEnumerable<int> GetCreativeValues()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < 12; i++)
		{
			list.Add(SetBlockID(Mats[i], isExplosion: false, isUnstable: false));
		}
		return list;
	}

	public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
	{
		return new BlockDebrisParticleSystem(subsystemTerrain, position, 1f, 1f, Color.White, GetFaceTextureSlot(4, value), IndustrialModLoader.IETexture);
	}

	public bool IsExplosion(int data)
	{
		return (data & 0x8000) != 0;
	}

	public bool IsUnstable(int data)
	{
		return (data & 0x10000) != 0;
	}

	public Materials GetMaterial(int data)
	{
		return (Materials)(data & 0x3FFF);
	}

	public int GetMatIndex(int data)
	{
		return data & 0x3FFF;
	}

	public static int SetBlockID(Materials material, bool isExplosion, bool isUnstable)
	{
		int num = (int)material;
		if (isExplosion)
		{
			num |= 0x8000;
		}
		if (isUnstable)
		{
			num |= 0x10000;
		}
		return Terrain.MakeBlockValue(Index, 0, num);
	}
}
