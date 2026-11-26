using System;
using Engine;
using Game;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemSourceOfFireBlockBehavior : SubsystemTorchBlockBehavior, IUpdateable
{
	protected Point3[] lightingPoints = new Point3[4];

	public SubsystemGameInfo m_subsystemGameInfo;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemExplosions m_subsystemExplosions;

	public override int[] HandledBlocks => new int[6]
	{
		BlocksManager.GetBlockIndex<WickerLampBlock>(),
		BlocksManager.GetBlockIndex<TorchBlock>(),
		BlocksManager.GetBlockIndex<MagmaBlock>(),
		BlocksManager.GetBlockIndex<FireBlock>(),
		BlocksManager.GetBlockIndex<JackOLanternBlock>(),
		BlocksManager.GetBlockIndex<CampfireBlock>()
	};

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
		m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
	}

	public void TryExplode(int x, int y, int z)
	{
		if (m_subsystemGameInfo.WorldSettings.EnvironmentBehaviorMode != EnvironmentBehaviorMode.Living)
		{
			return;
		}
		Terrain terrain = m_subsystemTerrain.Terrain;
		for (int i = x - 2; i <= x + 2; i++)
		{
			for (int j = y - 4; j <= y + 1; j++)
			{
				if (!terrain.IsCellValid(i, j, 0))
				{
					continue;
				}
				for (int k = z - 2; k <= z + 2; k++)
				{
					int cellValueFast = terrain.GetCellValueFast(i, j, k);
					if (Terrain.ExtractContents(cellValueFast) == CoalOreBlock.Index || Terrain.ExtractContents(cellValueFast) == PetroleumBlock.Index)
					{
						float pressure = 90f;
						if (terrain.GetCellContentsFast(i + 1, j, k) == 0)
						{
							m_subsystemExplosions.AddExplosion(i + 1, j, k, pressure, isIncendiary: false, noExplosionSound: false);
						}
						if (terrain.GetCellContentsFast(i - 1, j, k) == 0)
						{
							m_subsystemExplosions.AddExplosion(i - 1, j, k, pressure, isIncendiary: false, noExplosionSound: false);
						}
						if (terrain.GetCellContents(i, j + 1, k) == 0)
						{
							m_subsystemExplosions.AddExplosion(i, j + 1, k, pressure, isIncendiary: false, noExplosionSound: false);
						}
						if (terrain.GetCellContents(i, j - 1, k) == 0)
						{
							m_subsystemExplosions.AddExplosion(i, j - 1, k, pressure, isIncendiary: false, noExplosionSound: false);
						}
						if (terrain.GetCellContentsFast(i, j, k + 1) == 0)
						{
							m_subsystemExplosions.AddExplosion(i, j, k + 1, pressure, isIncendiary: false, noExplosionSound: false);
						}
						if (terrain.GetCellContentsFast(i, j, k - 1) == 0)
						{
							m_subsystemExplosions.AddExplosion(i, j, k - 1, pressure, isIncendiary: false, noExplosionSound: false);
						}
					}
				}
			}
		}
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		if (Terrain.ExtractContents(value) == 31 && Terrain.ExtractData(value) == 5)
		{
			return;
		}
		TryExplode(x, y, z);
		switch (Terrain.ExtractContents(value))
		{
		case 31:
			if (Terrain.ExtractData(value) == 5)
			{
				return;
			}
			break;
		case 92:
		case 104:
		case 209:
			return;
		}
		AddTorch(value, x, y, z);
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		RemoveTorch(new Point3(x, y, z));
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		RemoveTorch(new Point3(x, y, z));
		OnBlockAdded(value, oldValue, x, y, z);
	}

	public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
	{
		switch (Terrain.ExtractContents(value))
		{
		case 92:
		case 104:
		case 209:
			return;
		case 31:
			if (Terrain.ExtractData(value) == 5)
			{
				SubsystemTerrain.Terrain.SetCellValueFast(x, y, z, 0);
				return;
			}
			break;
		}
		AddTorch(value, x, y, z);
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		int cellValueFast = SubsystemTerrain.Terrain.GetCellValueFast(x, y, z);
		if (Terrain.ExtractContents(cellValueFast) != 31 || Terrain.ExtractData(cellValueFast) != 5)
		{
			base.OnNeighborBlockChanged(x, y, z, neighborX, neighborY, neighborZ);
		}
	}

	public void RemoveTorch(Point3 p)
	{
		if (m_particleSystemsByCell.TryGetValue(p, out var value))
		{
			m_subsystemParticles.RemoveParticleSystem(value);
			m_particleSystemsByCell.Remove(p);
		}
	}

	public void Update(float dt)
	{
	}

	public static Point3 GetPoint(Vector3 v)
	{
		return new Point3((int)MathF.Round(v.X), (int)MathF.Round(v.Y) + 1, (int)MathF.Round(v.Z));
	}
}
