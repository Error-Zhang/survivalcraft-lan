using Engine;
using Game;

namespace SCIENEW.Extensions.GameLogic;

public static class BlockEnvExtensions
{
	public static DrawBlockEnvironmentData GetEnvironmentData(this SubsystemTerrain subsystemTerrain, Vector3 position)
	{
		return new DrawBlockEnvironmentData().ModifyEnvironmentData(subsystemTerrain, position);
	}

	public static DrawBlockEnvironmentData ModifyEnvironmentData(this DrawBlockEnvironmentData environmentData, SubsystemTerrain subsystemTerrain, Vector3 position)
	{
		int x = Terrain.ToCell(position.X);
		int num = Terrain.ToCell(position.Y);
		int z = Terrain.ToCell(position.Z);
		environmentData.SubsystemTerrain = subsystemTerrain;
		TerrainChunk chunkAtCell = subsystemTerrain.Terrain.GetChunkAtCell(x, z);
		if (chunkAtCell != null && chunkAtCell.State >= TerrainChunkState.InvalidVertices1 && num >= 0 && num < 255)
		{
			environmentData.Humidity = subsystemTerrain.Terrain.GetSeasonalHumidity(x, z);
			environmentData.Temperature = subsystemTerrain.Terrain.GetSeasonalTemperature(x, z) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(num);
			float f = MathUtils.Max(position.Y - (float)num - 0.75f, 0f) / 0.25f;
			environmentData.Light = (int)MathUtils.Lerp(subsystemTerrain.Terrain.GetCellLightFast(x, num, z), subsystemTerrain.Terrain.GetCellLightFast(x, num + 1, z), f);
		}
		return environmentData;
	}

	public static DrawBlockEnvironmentData ModifyEnvironmentData(this DrawBlockEnvironmentData environmentData, SubsystemTerrain subsystemTerrain, Point3 coordinates)
	{
		return environmentData.ModifyEnvironmentData(subsystemTerrain, new Vector3(coordinates) + new Vector3(0.5f));
	}
}
