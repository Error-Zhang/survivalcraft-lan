using System.Collections.Generic;
using GameEntitySystem;
using SCIENEW.ProductionIO;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCreativeEngine : ComponentEngine
{
	public bool m_isPowering = true;

	public override IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		return ComponentEngine.m_emptySlotIndices;
	}

	protected override IEnumerable<int> GetTankProductionSlotIndices(ProductionSlotKind kind)
	{
		return ComponentEngine.m_emptyTankIndices;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_isPowering = valuesDictionary.GetValue("IsPowering", defaultValue: true);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("IsPowering", m_isPowering);
	}

	public override bool IsPowering()
	{
		return m_isPowering;
	}
}
