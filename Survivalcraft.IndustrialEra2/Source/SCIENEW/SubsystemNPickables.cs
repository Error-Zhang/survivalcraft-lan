using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemNPickables : Subsystem, IUpdateable
{
	public SubsystemUpdrafts m_subsystemUpdrafts;

	public SubsystemPickables m_subsystemPickables;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemUpdrafts = Project.FindSubsystem<SubsystemUpdrafts>(throwOnError: true);
		m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
	}

	public void Update(float dt)
	{
		double totalElapsedGameTime = m_subsystemPickables.m_subsystemGameInfo.TotalElapsedGameTime;
		foreach (Pickable pickable in m_subsystemPickables.Pickables)
		{
			if (pickable.ToRemove)
			{
				continue;
			}
			int num = m_subsystemPickables.Pickables.Count - m_subsystemPickables.m_pickablesToRemove.Count;
			float num2 = MathUtils.Lerp(300f, 90f, MathUtils.Saturate((float)num / 60f));
			if (totalElapsedGameTime - pickable.CreationTime <= (double)num2)
			{
				TerrainChunk chunkAtCell = m_subsystemPickables.m_subsystemTerrain.Terrain.GetChunkAtCell(Terrain.ToCell(pickable.Position.X), Terrain.ToCell(pickable.Position.Z));
				if (chunkAtCell != null && chunkAtCell.State > TerrainChunkState.InvalidContents4 && !pickable.FlyToPosition.HasValue)
				{
					Block block = BlocksManager.Blocks[m_subsystemPickables.m_subsystemTerrain.Terrain.GetCellContents(Terrain.ToCell(pickable.Position.X), Terrain.ToCell(pickable.Position.Y + 0.1f), Terrain.ToCell(pickable.Position.Z))];
					bool flag = ((block is MoltenIronBlock || block is MoltenSteelBlock) ? true : false);
					if (flag && !pickable.SplashGenerated)
					{
						m_subsystemPickables.m_subsystemParticles.AddParticleSystem(new MagmaSplashParticleSystem(m_subsystemPickables.m_subsystemTerrain, pickable.Position, large: false));
						m_subsystemPickables.m_subsystemAudio.PlayRandomSound("Audio/Sizzles", 1f, m_subsystemPickables.m_random.Float(-0.2f, 0.2f), pickable.Position, 3f, autoDelay: true);
						pickable.ToRemove = true;
						pickable.SplashGenerated = true;
						m_subsystemPickables.m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(pickable.Position.X), Terrain.ToCell(pickable.Position.Y), Terrain.ToCell(pickable.Position.Z), pickable.Value);
					}
				}
			}
			pickable.Velocity += m_subsystemUpdrafts.GetForce(pickable.Position) * dt * 60f;
		}
	}
}
