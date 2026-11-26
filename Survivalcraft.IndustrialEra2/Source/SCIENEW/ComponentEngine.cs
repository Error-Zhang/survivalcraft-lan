using System;
using System.Collections.Generic;
using Game;
using GameEntitySystem;
using SCIENEW.ProductionIO;
using TemplatesDatabase;

namespace SCIENEW;

public abstract class ComponentEngine : ComponentInventoryTankBase, IEngineComponent, IInventoryProductionSlots, ITankProductionSlots
{
	public enum EngineType
	{
		Block,
		Vehicle
	}

	protected static readonly int[] m_consumeSlotIndices = new int[1];

	protected static readonly int[] m_inputTankIndices = new int[1];

	protected static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	protected static readonly int[] m_emptyTankIndices = Array.Empty<int>();

	public ComponentBlockEntity? m_componentBlockEntity;

	public ComponentBody? m_componentBody;

	public SubsystemEnginePower m_subsystemEnginePower;

	public SubsystemAudio m_subsystemAudio;

	public SubsystemTime m_subsystemTime;

	public float m_audioVolume;

	public EngineType m_type;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: false);
		m_componentBody = Entity.FindComponent<ComponentBody>(throwOnError: false);
		m_subsystemEnginePower = Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		if (m_componentBlockEntity == null && m_componentBody != null)
		{
			m_type = EngineType.Vehicle;
		}
		else if (m_componentBlockEntity != null && m_componentBody == null)
		{
			m_type = EngineType.Block;
		}
		UpdateVolume();
	}

	public virtual bool IsPowering()
	{
		return false;
	}

	public override void OnEntityAdded()
	{
		base.OnEntityAdded();
		AddEngine();
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		RemoveEngine();
	}

	public virtual void AddEngine()
	{
		if (m_type == EngineType.Block)
		{
			m_subsystemEnginePower.Add(m_componentBlockEntity.Coordinates, this);
		}
	}

	public virtual void RemoveEngine()
	{
		if (m_type == EngineType.Block)
		{
			m_subsystemEnginePower.Remove(m_componentBlockEntity.Coordinates);
		}
	}

	public void UpdateVolume()
	{
		m_audioVolume = IESettingsManager.Get<float>("MachinesVolume");
	}

	public virtual IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		if (kind == ProductionSlotKind.Consume)
		{
			return m_consumeSlotIndices;
		}
		return m_emptySlotIndices;
	}

	protected virtual IEnumerable<int> GetTankProductionSlotIndices(ProductionSlotKind kind)
	{
		if (kind == ProductionSlotKind.Input)
		{
			return m_inputTankIndices;
		}
		return m_emptyTankIndices;
	}

	IEnumerable<int> ITankProductionSlots.GetSlotIndices(ProductionSlotKind kind)
	{
		return GetTankProductionSlotIndices(kind);
	}
}
