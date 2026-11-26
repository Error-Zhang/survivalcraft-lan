using Engine;
using Game;
using TemplatesDatabase;

namespace SCIENEW;

internal class SubsystemCreatureSpawnIE : SubsystemCreatureSpawn, IUpdateable
{
	public SubsystemTimeOfDay m_subsystemTimeOfDay;

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemTimeOfDay = Project.FindSubsystem<SubsystemTimeOfDay>(throwOnError: true);
	}

	public override void InitializeCreatureTypes()
	{
		m_creatureTypes.Add(new CreatureType("T-100", SpawnLocationType.Surface, randomSpawn: false, constantSpawn: false)
		{
			SpawnSuitabilityFunction = (CreatureType creatureType, Point3 point) =>
			{
				float num = m_subsystemTerrain.TerrainContentsGenerator.CalculateOceanShoreDistance(point.X, point.Z);
				m_subsystemTerrain.Terrain.GetTemperature(point.X, point.Z);
				m_subsystemTerrain.Terrain.GetHumidity(point.X, point.Z);
				int num2 = Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValueFast(point.X, point.Y - 1, point.Z));
				return (!(num > 40f) || !m_subsystemGameInfo.WorldSettings.AreSupernaturalCreaturesEnabled || (num2 != 8 && num2 != 2 && num2 != 3 && num2 != 7 && num2 != 12) || !(m_subsystemTimeOfDay.Day > 600.0)) ? 0f : 0.15f;
			},
			SpawnFunction = (CreatureType creatureType, Point3 point) => SpawnCreatures(creatureType, "T-100", point, 1).Count
		});
		m_creatureTypes.Add(new CreatureType("T-200", SpawnLocationType.Surface, randomSpawn: false, constantSpawn: false)
		{
			SpawnSuitabilityFunction = (CreatureType creatureType, Point3 point) =>
			{
				float num = m_subsystemTerrain.TerrainContentsGenerator.CalculateOceanShoreDistance(point.X, point.Z);
				m_subsystemTerrain.Terrain.GetTemperature(point.X, point.Z);
				m_subsystemTerrain.Terrain.GetHumidity(point.X, point.Z);
				int num2 = Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValueFast(point.X, point.Y - 1, point.Z));
				return (!(num > 60f) || !m_subsystemGameInfo.WorldSettings.AreSupernaturalCreaturesEnabled || (num2 != 8 && num2 != 2 && num2 != 3 && num2 != 7 && num2 != 12) || !(m_subsystemTimeOfDay.Day > 1000.0)) ? 0f : 0.15f;
			},
			SpawnFunction = (CreatureType creatureType, Point3 point) => SpawnCreatures(creatureType, "T-200", point, 1).Count
		});
		base.InitializeCreatureTypes();
	}

	public new void Update(float dt)
	{
		base.Update(dt);
	}
}
