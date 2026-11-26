using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCondenserA : ComponentCondenser, ICondenserComponent
{
	public static float MinOutputPower = 0f;

	public static float MaxOutputPower = 1024f;

	public override float Capacitance => 50000f;

	public new float OutputPower { get; set; }

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		OutputPower = valuesDictionary.GetValue("OutputPower", 1f);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("OutputPower", OutputPower);
	}
}
