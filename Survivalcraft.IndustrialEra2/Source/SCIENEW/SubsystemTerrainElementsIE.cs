using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemTerrainElementsIE : Subsystem
{
	public ITerrainContentsGenerator terrainContentsGenerator;

	public SubsystemTerrain m_subsystemTerrain;

	private static List<TerrainBrush> goldBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> silverBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> platinumBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> leadBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> zincBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> stannaryBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> mercuryBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> chromiumBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> titaniumBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> nickelBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> uraniumBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> phosphorusBrushes = new List<TerrainBrush>();

	private static List<TerrainBrush> magnetiteBrushes = new List<TerrainBrush>();

	public static List<TerrainBrush> m_petroleumPocketBrushes = new List<TerrainBrush>();

	public bool TGCavesAndPockets
	{
		get
		{
			if (this.terrainContentsGenerator is TerrainContentsGenerator24 terrainContentsGenerator)
			{
				return terrainContentsGenerator.TGCavesAndPockets;
			}
			return false;
		}
	}

	public int Seed
	{
		get
		{
			if (this.terrainContentsGenerator is TerrainContentsGenerator24 terrainContentsGenerator)
			{
				return terrainContentsGenerator.m_seed;
			}
			return 0;
		}
	}

	public bool TGExtras
	{
		get
		{
			if (this.terrainContentsGenerator is TerrainContentsGenerator24 terrainContentsGenerator)
			{
				return terrainContentsGenerator.TGExtras;
			}
			return false;
		}
	}

	public void TerrainContentsGenerator24Initialize(ITerrainContentsGenerator terrainContentsGenerator, SubsystemTerrain subsystemTerrain)
	{
		if (terrainContentsGenerator is TerrainContentsGenerator24 terrainContentsGenerator2)
		{
			terrainContentsGenerator2.ChunkGenerationStep3.Add(new ChunkGenerationStep(6000, GenerateMineralsIE));
			terrainContentsGenerator2.ChunkGenerationStep3.Add(new ChunkGenerationStep(6100, GeneratePocketsIE));
			terrainContentsGenerator2.ChunkGenerationStep4.Add(new ChunkGenerationStep(6000, GenerateFlax));
			terrainContentsGenerator2.ChunkGenerationStep4.Add(new ChunkGenerationStep(6100, GeneratePolymetallicNodule));
		}
	}

	public override void Load(ValuesDictionary valuesDictionary)
	{
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		terrainContentsGenerator = m_subsystemTerrain.TerrainContentsGenerator;
		ClearBrushesIE();
		CreateBrushesIE();
	}

	public void GenerateMineralsIE(TerrainChunk chunk)
	{
		if (!TGCavesAndPockets)
		{
			return;
		}
		int x = chunk.Coords.X;
		int y = chunk.Coords.Y;
		for (int i = x - 1; i <= x + 1; i++)
		{
			for (int j = y - 1; j <= y + 1; j++)
			{
				Game.Random random = new Game.Random(Seed + i + 10086 * j);
				int num = random.Int(0, 10);
				for (int k = 0; k < num; k++)
				{
					random.Int(0, 1);
				}
				float num2 = terrainContentsGenerator.CalculateMountainRangeFactor(i * 16, j * 16);
				int num3 = (int)(OresDataBase.GetOreProbability(Materials.Gold) + 2f * num2 * SimplexNoise.OctavedNoise(i + 12311, j + 12464, 0.33f, 1, 1f, 1f));
				for (int l = 0; l < num3; l++)
				{
					int x2 = i * 16 + random.Int(0, 15);
					int y2 = random.Int(2, 40);
					int z = j * 16 + random.Int(0, 15);
					goldBrushes[random.Int(0, goldBrushes.Count - 1)].PaintFastSelective(chunk, x2, y2, z, 67);
				}
				int num4 = (int)(OresDataBase.GetOreProbability(Materials.Silver) + 2f * num2 * SimplexNoise.OctavedNoise(i + 1864, j + 28761, 0.33f, 1, 1f, 1f));
				for (int m = 0; m < num4; m++)
				{
					int x3 = i * 16 + random.Int(0, 15);
					int y3 = random.Int(2, 40);
					int z2 = j * 16 + random.Int(0, 15);
					silverBrushes[random.Int(0, silverBrushes.Count - 1)].PaintFastSelective(chunk, x3, y3, z2, 67);
				}
				int num5 = (int)(OresDataBase.GetOreProbability(Materials.Platinum) + 2f * num2 * SimplexNoise.OctavedNoise(i + 12353454, j + 261, 0.33f, 1, 1f, 1f));
				for (int n = 0; n < num5; n++)
				{
					int x4 = i * 16 + random.Int(0, 15);
					int y4 = random.Int(2, 40);
					int z3 = j * 16 + random.Int(0, 15);
					platinumBrushes[random.Int(0, platinumBrushes.Count - 1)].PaintFastSelective(chunk, x4, y4, z3, 67);
				}
				int num6 = (int)(OresDataBase.GetOreProbability(Materials.Lead) + 3f * num2 * SimplexNoise.OctavedNoise(i + 1232, j + 8642, 0.33f, 1, 1f, 1f));
				for (int num7 = 0; num7 < num6; num7++)
				{
					int x5 = i * 16 + random.Int(0, 15);
					int y5 = random.Int(5, 200);
					int z4 = j * 16 + random.Int(0, 15);
					leadBrushes[random.Int(0, leadBrushes.Count - 1)].PaintFastSelective(chunk, x5, y5, z4, 3);
				}
				int num8 = (int)(OresDataBase.GetOreProbability(Materials.Zinc) + 3f * num2 * SimplexNoise.OctavedNoise(i, j, 0.33f, 1, 1f, 1f));
				for (int num9 = 0; num9 < num8; num9++)
				{
					int x6 = i * 16 + random.Int(0, 15);
					int y6 = random.Int(5, 200);
					int z5 = j * 16 + random.Int(0, 15);
					zincBrushes[random.Int(0, zincBrushes.Count - 1)].PaintFastSelective(chunk, x6, y6, z5, 3);
				}
				int num10 = (int)(OresDataBase.GetOreProbability(Materials.Stannary) + 3f * num2 * SimplexNoise.OctavedNoise(i + 6442, j + 2437, 0.33f, 1, 1f, 1f));
				for (int num11 = 0; num11 < num10; num11++)
				{
					int x7 = i * 16 + random.Int(0, 15);
					int y7 = random.Int(5, 200);
					int z6 = j * 16 + random.Int(0, 15);
					stannaryBrushes[random.Int(0, stannaryBrushes.Count - 1)].PaintFastSelective(chunk, x7, y7, z6, 3);
				}
				int num12 = (int)(OresDataBase.GetOreProbability(Materials.Mercury) + 2f * num2 * SimplexNoise.OctavedNoise(i + 5597, j + 122, 0.33f, 1, 1f, 1f));
				for (int num13 = 0; num13 < num12; num13++)
				{
					int x8 = i * 16 + random.Int(0, 15);
					int y8 = random.Int(2, 40);
					int z7 = j * 16 + random.Int(0, 15);
					mercuryBrushes[random.Int(0, mercuryBrushes.Count - 1)].PaintFastSelective(chunk, x8, y8, z7, 67);
				}
				int num14 = (int)(OresDataBase.GetOreProbability(Materials.Chromium) + 2f * num2 * SimplexNoise.OctavedNoise(i + 1231, j + 1241, 0.33f, 1, 1f, 1f));
				for (int num15 = 0; num15 < num14; num15++)
				{
					int x9 = i * 16 + random.Int(0, 15);
					int y9 = random.Int(2, 40);
					int z8 = j * 16 + random.Int(0, 15);
					chromiumBrushes[random.Int(0, chromiumBrushes.Count - 1)].PaintFastSelective(chunk, x9, y9, z8, 67);
				}
				int num16 = (int)(OresDataBase.GetOreProbability(Materials.Titanium) + 2f * num2 * SimplexNoise.OctavedNoise(i + 1214, j + 18764, 0.33f, 1, 1f, 1f));
				for (int num17 = 0; num17 < num16; num17++)
				{
					int x10 = i * 16 + random.Int(0, 15);
					int y10 = random.Int(2, 40);
					int z9 = j * 16 + random.Int(0, 15);
					titaniumBrushes[random.Int(0, titaniumBrushes.Count - 1)].PaintFastSelective(chunk, x10, y10, z9, 67);
				}
				int num18 = (int)(OresDataBase.GetOreProbability(Materials.Nickel) + 2f * num2 * SimplexNoise.OctavedNoise(i + 8523, j + 9564, 0.33f, 1, 1f, 1f));
				for (int num19 = 0; num19 < num18; num19++)
				{
					int x11 = i * 16 + random.Int(0, 15);
					int y11 = random.Int(2, 40);
					int z10 = j * 16 + random.Int(0, 15);
					nickelBrushes[random.Int(0, nickelBrushes.Count - 1)].PaintFastSelective(chunk, x11, y11, z10, 67);
				}
				int num20 = (int)(OresDataBase.GetOreProbability(Materials.Uranium) + 2f * num2 * SimplexNoise.OctavedNoise(i + 742, j + 98, 0.33f, 1, 1f, 1f));
				for (int num21 = 0; num21 < num20; num21++)
				{
					int x12 = i * 16 + random.Int(0, 15);
					int y12 = random.Int(2, 40);
					int z11 = j * 16 + random.Int(0, 15);
					uraniumBrushes[random.Int(0, uraniumBrushes.Count - 1)].PaintFastSelective(chunk, x12, y12, z11, 67);
				}
				int num22 = (int)(OresDataBase.GetOreProbability(Materials.Phosphorus) + 3f * num2 * SimplexNoise.OctavedNoise(i + 6295, j + 345, 0.33f, 1, 1f, 1f));
				for (int num23 = 0; num23 < num22; num23++)
				{
					int x13 = i * 16 + random.Int(0, 15);
					int y13 = random.Int(5, 200);
					int z12 = j * 16 + random.Int(0, 15);
					phosphorusBrushes[random.Int(0, phosphorusBrushes.Count - 1)].PaintFastSelective(chunk, x13, y13, z12, 3);
				}
				int num24 = (int)(OresDataBase.GetOreProbability(Materials.Iron) + 2f * num2 * SimplexNoise.OctavedNoise(i + 1736, j + 98, 0.33f, 1, 1f, 1f));
				for (int num25 = 0; num25 < num24; num25++)
				{
					int x14 = i * 16 + random.Int(0, 15);
					int y14 = random.Int(2, 40);
					int z13 = j * 16 + random.Int(0, 15);
					magnetiteBrushes[random.Int(0, magnetiteBrushes.Count - 1)].PaintFastSelective(chunk, x14, y14, z13, 67);
				}
			}
		}
	}

	public void GeneratePocketsIE(TerrainChunk chunk)
	{
		if (!TGCavesAndPockets || terrainContentsGenerator.CalculateOceanShoreDistance(chunk.Origin.X, chunk.Origin.Y) <= 50f)
		{
			return;
		}
		int x = chunk.Coords.X;
		int y = chunk.Coords.Y;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				int num = x + i;
				int num2 = y + j;
				Game.Random random = new Game.Random(Seed + num + 7698 * num2);
				if (random.Int(0, 4) == 0)
				{
					continue;
				}
				float num3 = terrainContentsGenerator.CalculateMountainRangeFactor(num * 16, num2 * 16);
				if (random.Bool(0.02f + 0.01f * num3))
				{
					int num4 = num * 16;
					int y2 = random.Int(40, 62);
					int num5 = num2 * 16;
					int num6 = random.Int(1, 3);
					for (int k = 0; k < num6; k++)
					{
						Vector2 vector = random.Vector2(7f);
						int num7 = 8 + (int)MathF.Round(vector.X);
						int num8 = 8 + (int)MathF.Round(vector.Y);
						int index = random.Int(0, m_petroleumPocketBrushes.Count - 1);
						m_petroleumPocketBrushes[index].PaintFast(chunk, num4 + num7, y2, num5 + num8);
					}
				}
			}
		}
	}

	public void GenerateFlax(TerrainChunk chunk)
	{
		if (!TGExtras)
		{
			return;
		}
		Game.Random random = new Game.Random(Seed + chunk.Coords.X + 1659 * chunk.Coords.Y);
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int num = 250; num >= 0; num--)
				{
					int cellValueFast = chunk.GetCellValueFast(i, num, j);
					int cellValueFast2 = chunk.GetCellValueFast(i, num + 1, j);
					int cellValueFast3 = chunk.GetCellValueFast(i, num + 2, j);
					int cellValueFast4 = chunk.GetCellValueFast(i, num + 3, j);
					int num2 = Terrain.ExtractContents(cellValueFast);
					int num3 = Terrain.ExtractContents(cellValueFast2);
					int num4 = Terrain.ExtractContents(cellValueFast3);
					int num5 = Terrain.ExtractContents(cellValueFast4);
					if (num2 != AirBlock.Index)
					{
						if (num2 == BlocksManager.GetBlockIndex<GrassBlock>() && num3 == BlocksManager.GetBlockIndex<AirBlock>() && num4 == BlocksManager.GetBlockIndex<AirBlock>() && num5 == BlocksManager.GetBlockIndex<AirBlock>())
						{
							int temperatureFast = chunk.GetTemperatureFast(i, j);
							int humidityFast = chunk.GetHumidityFast(i, j);
							GenerateFlaxAtPoint(chunk, temperatureFast, humidityFast, random, new Point3(i, num, j));
						}
						break;
					}
				}
			}
		}
	}

	private void GenerateFlaxAtPoint(TerrainChunk chunk, float temperature, float humidity, Game.Random random, Point3 point)
	{
		float num = random.Float(0f, 1f);
		float num2 = temperature / 15f;
		float num3 = humidity / 15f;
		if (!(random.Float(0f, 1f) > num3) && !(random.Float(0f, 1f) > ((num2 > 0.5f) ? (2f - 2f * num2) : (2f * num2))) && num < 0.01f)
		{
			chunk.SetCellValueFast(point.X, point.Y + 1, point.Z, NormalBlockManager.GetNormalBlockID("Flax_Stem_Ripened"));
			chunk.SetCellValueFast(point.X, point.Y + 2, point.Z, NormalBlockManager.GetNormalBlockID("Flax_Stem_Ripened"));
			chunk.SetCellValueFast(point.X, point.Y + 3, point.Z, NormalBlockManager.GetNormalBlockID("Flax_Top"));
		}
	}

	public void GeneratePolymetallicNodule(TerrainChunk chunk)
	{
		if (!TGExtras)
		{
			return;
		}
		int x = chunk.Coords.X;
		int y = chunk.Coords.Y;
		Game.Random random = new Game.Random(0);
		float num = 0f;
		for (int i = 0; i < 9; i++)
		{
			int num2 = i % 3 - 1;
			int num3 = i / 3 - 1;
			random.Seed(Seed + x + num2 + 2639 * (y + num3));
			if (random.Bool(0.2f))
			{
				num = MathUtils.Max(num, 0.015f);
				if (i == 4)
				{
					num = MathUtils.Max(num, 0.05f);
				}
			}
		}
		if (num == 0f)
		{
			return;
		}
		random.Seed(Seed + x + 2639 * y);
		int num4 = random.Int(0, MathUtils.Max((int)(8f * num), 1));
		for (int j = 0; j < num4; j++)
		{
			int num5 = random.Int(2, 13);
			int num6 = random.Int(2, 13);
			int num7 = num5 + chunk.Origin.X;
			int num8 = num6 + chunk.Origin.Y;
			int num9 = random.Int(10, 26);
			if (terrainContentsGenerator.CalculateOceanShoreDistance(num7, num8) > -10f)
			{
				continue;
			}
			int num10 = 40;
			int num11 = 10;
			for (int k = 0; k < num9; k++)
			{
				int x2 = num5 + random.Int(-2, 2);
				int z = num6 + random.Int(-2, 2);
				int num12 = 0;
				for (int num13 = 254; num13 >= 0; num13--)
				{
					int num14 = Terrain.ExtractContents(chunk.GetCellValueFast(x2, num13, z));
					if (num14 != 0)
					{
						if (num14 != WaterBlock.Index)
						{
							if (num12 >= num11 && (num14 == 2 || num14 == 7))
							{
								chunk.SetCellValueFast(x2, num13, z, NormalBlockManager.GetNormalBlockID("PolymetallicNodule"));
							}
							break;
						}
						num12++;
						if (num12 > num10)
						{
							break;
						}
					}
				}
			}
		}
	}

	public static void ClearBrushesIE()
	{
		goldBrushes.Clear();
		silverBrushes.Clear();
		platinumBrushes.Clear();
		leadBrushes.Clear();
		zincBrushes.Clear();
		stannaryBrushes.Clear();
		mercuryBrushes.Clear();
		chromiumBrushes.Clear();
		titaniumBrushes.Clear();
		nickelBrushes.Clear();
		uraniumBrushes.Clear();
		phosphorusBrushes.Clear();
		magnetiteBrushes.Clear();
	}

	public static void CreateBrushesIE()
	{
		Game.Random random = new Game.Random(37);
		for (int i = 0; i < 16; i++)
		{
			TerrainBrush terrainBrush = new TerrainBrush();
			int num = random.Int(4, 12);
			for (int j = 0; j < num; j++)
			{
				Vector3 vector = 0.5f * new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)).Normalize();
				int num2 = random.Int(3, 8);
				Vector3 zero = Vector3.Zero;
				for (int k = 0; k < num2; k++)
				{
					terrainBrush.AddBox((int)MathF.Floor(zero.X), (int)MathF.Floor(zero.Y), (int)MathF.Floor(zero.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Zinc, isExplosion: false, isUnstable: false));
					zero += vector;
				}
			}
			terrainBrush.Compile();
			zincBrushes.Add(terrainBrush);
		}
		for (int l = 0; l < 16; l++)
		{
			TerrainBrush terrainBrush2 = new TerrainBrush();
			int num3 = random.Int(3, 7);
			for (int m = 0; m < num3; m++)
			{
				Vector3 vector2 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num4 = random.Int(1, 4);
				Vector3 zero2 = Vector3.Zero;
				for (int n = 0; n < num4; n++)
				{
					terrainBrush2.AddBox((int)MathF.Floor(zero2.X), (int)MathF.Floor(zero2.Y), (int)MathF.Floor(zero2.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Gold, isExplosion: false, isUnstable: false));
					zero2 += vector2;
				}
			}
			terrainBrush2.Compile();
			goldBrushes.Add(terrainBrush2);
		}
		for (int num5 = 0; num5 < 16; num5++)
		{
			TerrainBrush terrainBrush3 = new TerrainBrush();
			int num6 = random.Int(3, 7);
			for (int num7 = 0; num7 < num6; num7++)
			{
				Vector3 vector3 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num8 = random.Int(2, 5);
				Vector3 zero3 = Vector3.Zero;
				for (int num9 = 0; num9 < num8; num9++)
				{
					terrainBrush3.AddBox((int)MathF.Floor(zero3.X), (int)MathF.Floor(zero3.Y), (int)MathF.Floor(zero3.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Silver, isExplosion: false, isUnstable: false));
					zero3 += vector3;
				}
			}
			terrainBrush3.Compile();
			silverBrushes.Add(terrainBrush3);
		}
		for (int num10 = 0; num10 < 16; num10++)
		{
			TerrainBrush terrainBrush4 = new TerrainBrush();
			int num11 = random.Int(3, 7);
			for (int num12 = 0; num12 < num11; num12++)
			{
				Vector3 vector4 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num13 = random.Int(1, 4);
				Vector3 zero4 = Vector3.Zero;
				for (int num14 = 0; num14 < num13; num14++)
				{
					terrainBrush4.AddBox((int)MathF.Floor(zero4.X), (int)MathF.Floor(zero4.Y), (int)MathF.Floor(zero4.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Platinum, isExplosion: false, isUnstable: false));
					zero4 += vector4;
				}
			}
			terrainBrush4.Compile();
			platinumBrushes.Add(terrainBrush4);
		}
		for (int num15 = 0; num15 < 16; num15++)
		{
			TerrainBrush terrainBrush5 = new TerrainBrush();
			int num16 = random.Int(4, 12);
			for (int num17 = 0; num17 < num16; num17++)
			{
				Vector3 vector5 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num18 = random.Int(3, 8);
				Vector3 zero5 = Vector3.Zero;
				for (int num19 = 0; num19 < num18; num19++)
				{
					terrainBrush5.AddBox((int)MathF.Floor(zero5.X), (int)MathF.Floor(zero5.Y), (int)MathF.Floor(zero5.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Lead, isExplosion: false, isUnstable: false));
					zero5 += vector5;
				}
			}
			terrainBrush5.Compile();
			leadBrushes.Add(terrainBrush5);
		}
		for (int num20 = 0; num20 < 16; num20++)
		{
			TerrainBrush terrainBrush6 = new TerrainBrush();
			int num21 = random.Int(4, 12);
			for (int num22 = 0; num22 < num21; num22++)
			{
				Vector3 vector6 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num23 = random.Int(3, 8);
				Vector3 zero6 = Vector3.Zero;
				for (int num24 = 0; num24 < num23; num24++)
				{
					terrainBrush6.AddBox((int)MathF.Floor(zero6.X), (int)MathF.Floor(zero6.Y), (int)MathF.Floor(zero6.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Stannary, isExplosion: false, isUnstable: false));
					zero6 += vector6;
				}
			}
			terrainBrush6.Compile();
			stannaryBrushes.Add(terrainBrush6);
		}
		for (int num25 = 0; num25 < 16; num25++)
		{
			TerrainBrush terrainBrush7 = new TerrainBrush();
			int num26 = random.Int(3, 7);
			for (int num27 = 0; num27 < num26; num27++)
			{
				Vector3 vector7 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num28 = random.Int(3, 6);
				Vector3 zero7 = Vector3.Zero;
				for (int num29 = 0; num29 < num28; num29++)
				{
					terrainBrush7.AddBox((int)MathF.Floor(zero7.X), (int)MathF.Floor(zero7.Y), (int)MathF.Floor(zero7.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Chromium, isExplosion: false, isUnstable: false));
					zero7 += vector7;
				}
			}
			terrainBrush7.Compile();
			chromiumBrushes.Add(terrainBrush7);
		}
		for (int num30 = 0; num30 < 16; num30++)
		{
			TerrainBrush terrainBrush8 = new TerrainBrush();
			int num31 = random.Int(3, 7);
			for (int num32 = 0; num32 < num31; num32++)
			{
				Vector3 vector8 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num33 = random.Int(3, 6);
				Vector3 zero8 = Vector3.Zero;
				for (int num34 = 0; num34 < num33; num34++)
				{
					terrainBrush8.AddBox((int)MathF.Floor(zero8.X), (int)MathF.Floor(zero8.Y), (int)MathF.Floor(zero8.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Mercury, isExplosion: false, isUnstable: false));
					zero8 += vector8;
				}
			}
			terrainBrush8.Compile();
			mercuryBrushes.Add(terrainBrush8);
		}
		for (int num35 = 0; num35 < 16; num35++)
		{
			TerrainBrush terrainBrush9 = new TerrainBrush();
			int num36 = random.Int(3, 7);
			for (int num37 = 0; num37 < num36; num37++)
			{
				Vector3 vector9 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num38 = random.Int(3, 6);
				Vector3 zero9 = Vector3.Zero;
				for (int num39 = 0; num39 < num38; num39++)
				{
					terrainBrush9.AddBox((int)MathF.Floor(zero9.X), (int)MathF.Floor(zero9.Y), (int)MathF.Floor(zero9.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Titanium, isExplosion: false, isUnstable: false));
					zero9 += vector9;
				}
			}
			terrainBrush9.Compile();
			titaniumBrushes.Add(terrainBrush9);
		}
		for (int num40 = 0; num40 < 16; num40++)
		{
			TerrainBrush terrainBrush10 = new TerrainBrush();
			int num41 = random.Int(3, 7);
			for (int num42 = 0; num42 < num41; num42++)
			{
				Vector3 vector10 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num43 = random.Int(3, 6);
				Vector3 zero10 = Vector3.Zero;
				for (int num44 = 0; num44 < num43; num44++)
				{
					terrainBrush10.AddBox((int)MathF.Floor(zero10.X), (int)MathF.Floor(zero10.Y), (int)MathF.Floor(zero10.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Nickel, isExplosion: false, isUnstable: false));
					zero10 += vector10;
				}
			}
			terrainBrush10.Compile();
			nickelBrushes.Add(terrainBrush10);
		}
		for (int num45 = 0; num45 < 16; num45++)
		{
			TerrainBrush terrainBrush11 = new TerrainBrush();
			int num46 = random.Int(3, 7);
			for (int num47 = 0; num47 < num46; num47++)
			{
				Vector3 vector11 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num48 = random.Int(3, 6);
				Vector3 zero11 = Vector3.Zero;
				for (int num49 = 0; num49 < num48; num49++)
				{
					terrainBrush11.AddBox((int)MathF.Floor(zero11.X), (int)MathF.Floor(zero11.Y), (int)MathF.Floor(zero11.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Uranium, isExplosion: false, isUnstable: false));
					zero11 += vector11;
				}
			}
			terrainBrush11.Compile();
			uraniumBrushes.Add(terrainBrush11);
		}
		for (int num50 = 0; num50 < 16; num50++)
		{
			TerrainBrush terrainBrush12 = new TerrainBrush();
			int num51 = random.Int(3, 7);
			for (int num52 = 0; num52 < num51; num52++)
			{
				Vector3 vector12 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num53 = random.Int(3, 6);
				Vector3 zero12 = Vector3.Zero;
				for (int num54 = 0; num54 < num53; num54++)
				{
					terrainBrush12.AddBox((int)MathF.Floor(zero12.X), (int)MathF.Floor(zero12.Y), (int)MathF.Floor(zero12.Z), 1, 1, 1, NormalBlockManager.GetNormalBlockID("Magnetite"));
					zero12 += vector12;
				}
			}
			terrainBrush12.Compile();
			magnetiteBrushes.Add(terrainBrush12);
		}
		for (int num55 = 0; num55 < 16; num55++)
		{
			TerrainBrush terrainBrush13 = new TerrainBrush();
			int num56 = random.Int(4, 12);
			for (int num57 = 0; num57 < num56; num57++)
			{
				Vector3 vector13 = 0.5f * Vector3.Normalize(new Vector3(random.Float(-1f, 1f), random.Float(-1f, 1f), random.Float(-1f, 1f)));
				int num58 = random.Int(3, 8);
				Vector3 zero13 = Vector3.Zero;
				for (int num59 = 0; num59 < num58; num59++)
				{
					terrainBrush13.AddBox((int)MathF.Floor(zero13.X), (int)MathF.Floor(zero13.Y), (int)MathF.Floor(zero13.Z), 1, 1, 1, OresBlock.SetBlockID(Materials.Phosphorus, isExplosion: false, isUnstable: false));
					zero13 += vector13;
				}
			}
			terrainBrush13.Compile();
			phosphorusBrushes.Add(terrainBrush13);
		}
		int[] array = new int[3] { 10, 14, 24 };
		for (int num60 = 0; num60 < 4 * array.Length; num60++)
		{
			TerrainBrush terrainBrush14 = new TerrainBrush();
			int num61 = array[num60 / 4];
			int num62 = num60 % 2 + 1;
			float num63 = ((num60 % 4 == 2) ? 0.5f : 1f);
			int num64 = ((num60 % 4 == 1) ? (num61 * num61) : (2 * num61 * num61));
			for (int num65 = 0; num65 < num64; num65++)
			{
				Vector2 vector14 = random.Vector2(0f, num61);
				float num66 = vector14.Length();
				int num67 = random.Int(3, 4);
				int sizeY = 1 + (int)MathUtils.Lerp(MathUtils.Max(num61 / 3, 2.5f) * num63, 0f, num66 / (float)num61) + random.Int(0, 1);
				terrainBrush14.AddBox((int)MathF.Floor(vector14.X), 0, (int)MathF.Floor(vector14.Y), num67, sizeY, num67, 0);
				terrainBrush14.AddBox((int)MathF.Floor(vector14.X), -num62, (int)MathF.Floor(vector14.Y), num67, num62, num67, PetroleumBlock.Index);
			}
			terrainBrush14.Compile();
			m_petroleumPocketBrushes.Add(terrainBrush14);
		}
	}
}
