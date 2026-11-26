using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCreativeGenerator : Component
{
	public float m_voltage;

	public float m_power;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_voltage = valuesDictionary.GetValue("Voltage", 1f);
		m_power = valuesDictionary.GetValue("Power", 3f);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("Voltage", m_voltage);
		valuesDictionary.SetValue("Power", m_power);
	}
}
