using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCheckValve : ComponentTankBase
{
	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_tanks[0].Capacity = 1f;
	}

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (!(FluidsManager.GetFluid(value) is IPipeableFluid))
		{
			return 0f;
		}
		return base.GetTankCapacity(tankIndex, value);
	}
}
