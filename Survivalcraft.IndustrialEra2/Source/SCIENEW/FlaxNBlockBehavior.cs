using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class FlaxNBlockBehavior : NormalBlockBehavior, IUpdateable
{
	public struct Placement(int value, Point3 owner)
	{
		public int Value = value;

		public Point3 Owner = owner;
	}

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemGameInfo m_subsystemGameInfo;

	public SubsystemTime m_subsystemTime;

	public Dictionary<Point3, Placement> m_toUpdate = new Dictionary<Point3, Placement>();

	private Game.Random m_random = new Game.Random();

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public FlaxNBlockBehavior(Project project)
		: base(project)
	{
		m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemGameInfo = project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
		m_subsystemTime = project.FindSubsystem<SubsystemTime>(throwOnError: true);
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		int cellValue = m_subsystemTerrain.Terrain.GetCellValue(x, y - 1, z);
		int num = Terrain.ExtractContents(cellValue);
		if (num != GrassBlock.Index && num != DirtBlock.Index && num != SoilBlock.Index && !(NormalBlockManager.GetNormalBlock(cellValue) is FlaxNBlock))
		{
			m_subsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
		}
	}

	public override void OnPoll(int value, int x, int y, int z, int pollPass)
	{
		if (m_subsystemGameInfo.WorldSettings.EnvironmentBehaviorMode != EnvironmentBehaviorMode.Living)
		{
			return;
		}
		int cellValue = m_subsystemTerrain.Terrain.GetCellValue(x, y + 1, z);
		if (Terrain.ExtractLight(cellValue) < 12 || !(m_random.Float(0f, 1f) < 0.5f))
		{
			return;
		}
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(x, y, z);
		m_subsystemTerrain.Terrain.GetCellValueFast(x, y - 1, z);
		int cellValueFast2 = m_subsystemTerrain.Terrain.GetCellValueFast(x, y - 2, z);
		switch (NormalBlockManager.GetNormalBlockCraftingID(cellValueFast))
		{
		case "Flax_Top":
			if (Terrain.ExtractContents(cellValue) == AirBlock.Index && !(NormalBlockManager.GetNormalBlock(cellValueFast2) is FlaxNBlock))
			{
				m_toUpdate.Add(new Point3(x, y, z), new Placement(NormalBlockManager.GetNormalBlockID("Flax_Stem"), new Point3(x, y, z)));
				m_toUpdate.Add(new Point3(x, y + 1, z), new Placement(NormalBlockManager.GetNormalBlockID("Flax_Top"), new Point3(x, y, z)));
			}
			break;
		case "Flax_Stem":
			m_toUpdate.Add(new Point3(x, y, z), new Placement(NormalBlockManager.GetNormalBlockID("Flax_Stem_Ripened"), new Point3(x, y, z)));
			break;
		case "Flax_Stem_Ripened":
			break;
		}
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		foreach (KeyValuePair<Point3, Placement> item in m_toUpdate)
		{
			if (item.Value.Owner == new Point3(x, y, z))
			{
				m_toUpdate.Remove(item.Key);
			}
		}
	}

	public void Update(float dt)
	{
		if (!m_subsystemTime.PeriodicGameTimeEvent(60.0, 0.0))
		{
			return;
		}
		foreach (KeyValuePair<Point3, Placement> item in m_toUpdate)
		{
			m_subsystemTerrain.ChangeCell(item.Key.X, item.Key.Y, item.Key.Z, item.Value.Value);
		}
		m_toUpdate.Clear();
	}
}
