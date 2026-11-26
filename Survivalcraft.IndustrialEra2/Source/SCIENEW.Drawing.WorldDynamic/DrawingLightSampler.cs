using Engine;
using Game;

namespace SCIENEW.Drawing.WorldDynamic;

public sealed class DrawingLightSampler
{
	private readonly SubsystemTerrain m_subsystemTerrain;

	public DrawingLightSampler(SubsystemTerrain subsystemTerrain)
	{
		m_subsystemTerrain = subsystemTerrain;
	}

	public float Sample(Vector3 position, float fallback = 1f)
	{
		int x = Terrain.ToCell(position.X);
		int num = Terrain.ToCell(position.Y);
		int z = Terrain.ToCell(position.Z);
		TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(x, z);
		bool flag;
		if (chunkAtCell != null)
		{
			TerrainChunkState state = chunkAtCell.State;
			if ((uint)(state - 5) <= 4u)
			{
				flag = true;
				goto IL_0051;
			}
		}
		flag = false;
		goto IL_0051;
		IL_0051:
		if (!flag)
		{
			return fallback;
		}
		float f = MathUtils.Max(position.Y - (float)num - 0.75f, 0f) / 0.25f;
		int cellLightFast = m_subsystemTerrain.Terrain.GetCellLightFast(x, num, z);
		int cellLightFast2 = m_subsystemTerrain.Terrain.GetCellLightFast(x, num + 1, z);
		int num2 = (int)MathUtils.Lerp(cellLightFast, cellLightFast2, f);
		return LightingManager.LightIntensityByLightValue[num2];
	}
}
