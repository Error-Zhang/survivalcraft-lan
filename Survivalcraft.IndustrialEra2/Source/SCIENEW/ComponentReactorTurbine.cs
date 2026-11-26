using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentReactorTurbine : Component, IUpdateable
{
	public const int MaxOutput = 64;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemTime m_subsystemTime;

	public List<ComponentHeatExchanger> m_componentHeatExchangers = new List<ComponentHeatExchanger>();

	public int Output { get; protected set; }

	public float OutputVoltage { get; protected set; } = 4f;

	public float OutputPower { get; protected set; }

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
	}

	public void FindExchanger()
	{
		m_componentHeatExchangers.Clear();
		for (int i = 0; i < 4; i++)
		{
			Point3 position = CellFace.FaceToPoint3(i) + m_componentBlockEntity.Coordinates;
			if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, position, out ComponentBlockEntity blockEntity))
			{
				ComponentHeatExchanger componentHeatExchanger = blockEntity.Entity.FindComponent<ComponentHeatExchanger>();
				if (componentHeatExchanger != null && CellFace.OppositeFace(i) == componentHeatExchanger.Direction)
				{
					m_componentHeatExchangers.Add(componentHeatExchanger);
				}
			}
		}
	}

	public void Update(float dt)
	{
		Output = 0;
		TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Z);
		bool flag;
		if (chunkAtCell != null)
		{
			TerrainChunkState state = chunkAtCell.State;
			if ((uint)(state - 5) <= 1u || state == TerrainChunkState.Valid)
			{
				flag = true;
				goto IL_0053;
			}
		}
		flag = false;
		goto IL_0053;
		IL_0053:
		if (flag && m_subsystemTime.PeriodicGameTimeEvent(0.20000000298023224, (double)GetHashCode() % 1000.0 / 1000.0))
		{
			FindExchanger();
		}
		if (m_componentHeatExchangers.Count == 0)
		{
			return;
		}
		foreach (ComponentHeatExchanger componentHeatExchanger in m_componentHeatExchangers)
		{
			Output += componentHeatExchanger.Output;
			Output = MathUtils.Min(64, Output);
		}
		OutputPower = Output;
		OutputVoltage = ((Output > 1) ? 4 : 0);
	}
}
