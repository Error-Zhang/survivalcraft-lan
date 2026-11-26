using Engine;
using Game;

namespace SCIENEW.Utils;

public static class TerrainUtils
{
	public static float GetAverageTemperatureAtChunk(SubsystemTerrain subsystemTerrain, int x, int y, int z, out bool chunkValid)
	{
		TerrainChunk chunkAtCell = subsystemTerrain.Terrain.GetChunkAtCell(x, z);
		float num = 0f;
		chunkValid = false;
		if (chunkAtCell != null && chunkAtCell.State >= TerrainChunkState.InvalidVertices1 && y >= 0 && y < 255)
		{
			chunkValid = true;
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					num += (float)(subsystemTerrain.Terrain.GetSeasonalTemperature(chunkAtCell.Origin.X + i, chunkAtCell.Origin.Y + j) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(y));
				}
			}
			return num / 256f;
		}
		return 0f;
	}

	public static float GetAverageTemperatureAtChunk(SubsystemTerrain subsystemTerrain, Point3 position, out bool chunkValid)
	{
		return GetAverageTemperatureAtChunk(subsystemTerrain, position.X, position.Y, position.Z, out chunkValid);
	}

	public static bool IsValid(TerrainChunk chunk)
	{
		if (chunk != null)
		{
			return chunk.State == TerrainChunkState.Valid;
		}
		return false;
	}
}
