using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentElectricBlastFurnace_Induction : ComponentInductionFurnace, IUpdateable
{
	public int m_tankIndex;

	public ComponentElectricBlastFurnace_Alloy m_alloyFurnace;

	public override float Speed => 1f;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_alloyFurnace = Entity.FindComponent<ComponentElectricBlastFurnace_Alloy>(throwOnError: true);
		m_tankIndex = valuesDictionary.GetValue<int>("TankIndex");
		m_tanks[0].Capacity = 32f;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("TankIndex", m_tankIndex);
	}

	public override void AddTankFluid(int tankIndex, int value, float volume, out float overflow)
	{
		m_alloyFurnace.m_updateCraftingRecipe = true;
		base.AddTankFluid(tankIndex, value, volume, out overflow);
	}

	public override float RemoveTankFluid(int tankIndex, float volume)
	{
		m_alloyFurnace.m_updateCraftingRecipe = true;
		return base.RemoveTankFluid(tankIndex, volume);
	}

	public override void UpdateWorkState()
	{
		m_isWorking = m_alloyFurnace.IsWorking;
		if (!m_alloyFurnace.IsWorking)
		{
			m_craftingProgress = 0f;
			m_craftingEquation = null;
			m_updateEquation = true;
		}
	}

	public override void ReplaceFurnace(Point3 coordinates)
	{
	}
}
