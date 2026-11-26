using System;
using System.Linq;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentUnpacker : ComponentInventoryTankBase, IUpdateable
{
	public float m_craftingProgress;

	public SubsystemVoltNet m_subsystemVoltNet;

	public ComponentBlockEntity m_componentBlockEntity;

	public BiDictionary<int, int> m_container = new BiDictionary<int, int>();

	public int m_fluidIndex;

	protected bool m_updateContainer;

	protected float m_speed = 0.25f;

	public float CraftingProgress => m_craftingProgress;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override float RemoveTankFluid(int tankIndex, float volume)
	{
		m_updateContainer = true;
		return base.RemoveTankFluid(tankIndex, volume);
	}

	public override void AddSlotItems(int slotIndex, int value, int count)
	{
		m_updateContainer = true;
		base.AddSlotItems(slotIndex, value, count);
	}

	public void Update(float dt)
	{
		if (m_updateContainer)
		{
			m_updateContainer = false;
			m_container = new BiDictionary<int, int>();
			m_fluidIndex = 0;
			if (GetSlotCount(0) > 0)
			{
				Fluid fluid = FluidsManager.Fluids.First((Fluid x) => x is IScoopableFluid scoopableFluid && scoopableFluid.AcquireContainer(FluidsManager.GetFluidID(x)).ContainsValue(GetSlotValue(0)));
				m_fluidIndex = FluidsManager.GetFluidID(fluid);
				m_container = ((IScoopableFluid)fluid).AcquireContainer(m_fluidIndex);
			}
		}
		if (m_subsystemVoltNet.GetVoltElement(m_componentBlockEntity.Coordinates) is UnpackerVoltElement { CurrentWorkState: WorkState.Active })
		{
			int slotValue = GetSlotValue(0);
			if (m_container.ContainsValue(slotValue) && ComponentInventoryTankBase.FindAcquireTankForFluid(this, m_fluidIndex) == 0 && ComponentInventoryBase.FindAcquireSlotForItem(this, m_container.GetBySecond(slotValue)) == 1)
			{
				m_craftingProgress = Math.Min(m_craftingProgress + dt * m_speed, 1f);
				if (m_craftingProgress == 1f)
				{
					m_craftingProgress = 0f;
					RemoveSlotItems(0, 1);
					AddSlotItems(1, m_container.GetBySecond(slotValue), 1);
					AddTankFluid(0, m_fluidIndex, FluidsManager.GetContainerVolume(slotValue), out var _);
				}
			}
			else
			{
				m_craftingProgress = 0f;
			}
		}
		else
		{
			m_craftingProgress = 0f;
		}
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_updateContainer = true;
		m_tanks[0].Capacity = 64f;
	}
}
