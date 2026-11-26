using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentEmptySpace : Component
{
	public Point3 m_root;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_root = valuesDictionary.GetValue("Root", Point3.Zero);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("Root", m_root);
	}
}
