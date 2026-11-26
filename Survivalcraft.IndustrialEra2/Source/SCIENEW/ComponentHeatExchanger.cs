using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentHeatExchanger : ComponentTankBase, IUpdateable
{
	public ComponentGReactorCore? m_componentReactor;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public ComponentBlockEntity m_componentBlockEntity;

	public int Pressure;

	public int Output;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int Direction { get; protected set; }

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (value != FluidsManager.GetFluidID("WaterFluid", 0))
		{
			return 0f;
		}
		return base.GetTankCapacity(tankIndex, value);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		Direction = valuesDictionary.GetValue("Direction", 0);
		m_tanks[0].Capacity = 64f;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("Direction", Direction);
	}

	public void FindReactor()
	{
		m_componentReactor = null;
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int cellValueFast = m_subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z);
		int num = (Direction = FourDirectionalDevice.GetDirection_Static(cellValueFast));
		int face = num;
		Point3 position = coordinates - CellFace.FaceToPoint3(face);
		if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, position, out ComponentBlockEntity blockEntity))
		{
			m_componentReactor = blockEntity.Entity.FindComponent<ComponentGReactorCore>();
		}
	}

	public void Update(float dt)
	{
		TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Z);
		bool flag;
		if (chunkAtCell != null)
		{
			TerrainChunkState state = chunkAtCell.State;
			if ((uint)(state - 5) <= 1u || state == TerrainChunkState.Valid)
			{
				flag = true;
				goto IL_004c;
			}
		}
		flag = false;
		goto IL_004c;
		IL_004c:
		if (flag && m_subsystemTime.PeriodicGameTimeEvent(0.20000000298023224, (double)GetHashCode() % 1000.0 / 1000.0))
		{
			FindReactor();
		}
		float num = 0.5f;
		if (!m_subsystemTime.PeriodicGameTimeEvent(num, (double)GetHashCode() % 1000.0 / 1000.0))
		{
			return;
		}
		Pressure = 0;
		Output = 0;
		if (m_componentReactor == null)
		{
			return;
		}
		Pressure = (int)(m_componentReactor.HeatLevel / 100f);
		if (Pressure > 0)
		{
			float num2 = 1f / (30f / (float)Pressure / 10f) / 4f * num;
			if (GetTankVolume(0) > num2 && GetTankValue(0) == FluidsManager.GetFluidID("WaterFluid", 0))
			{
				RemoveTankFluid(0, num2);
				Output += Pressure;
				m_componentReactor.HeatLevel -= 2 * Output;
			}
		}
	}
}
