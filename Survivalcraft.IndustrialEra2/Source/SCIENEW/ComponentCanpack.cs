using System;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCanpack : ComponentInventoryTankBase, IUpdateable
{
	public float m_craftingProgress;

	public SubsystemVoltNet m_subsystemVoltNet;

	public ComponentBlockEntity m_componentBlockEntity;

	public BiDictionary<int, int> m_container = new BiDictionary<int, int>();

	protected bool m_updateContainer;

	protected float m_speed = 0.25f;

	public float CraftingProgress => m_craftingProgress;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void AddTankFluid(int tankIndex, int value, float volume, out float overflow)
	{
		m_updateContainer = true;
		base.AddTankFluid(tankIndex, value, volume, out overflow);
	}

	public override int RemoveSlotItems(int slotIndex, int count)
	{
		if (GetTankVolume(0) <= 0f)
		{
			m_updateContainer = true;
		}
		return base.RemoveSlotItems(slotIndex, count);
	}

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (FluidsManager.GetFluid(value).GetFluidType(value) != FluidType.Liquid)
		{
			return 0f;
		}
		return base.GetTankCapacity(tankIndex, value);
	}

	public void Update(float dt)
	{
		if (m_updateContainer)
		{
			m_updateContainer = false;
			if (FluidsManager.GetFluid(GetTankValue(0)) is IScoopableFluid scoopableFluid)
			{
				m_container = scoopableFluid.AcquireContainer(GetTankValue(0));
			}
		}
		if (m_subsystemVoltNet.GetVoltElement(m_componentBlockEntity.Coordinates) is CanpackVoltElement { CurrentWorkState: WorkState.Active })
		{
			int slotValue = GetSlotValue(0);
			if (m_container.ContainsKey(slotValue) && ComponentInventoryBase.FindAcquireSlotForItem(this, m_container.GetByFirst(slotValue)) == 1 && GetTankVolume(0) >= FluidsManager.GetContainerVolume(slotValue))
			{
				m_craftingProgress = Math.Min(m_craftingProgress + dt * m_speed, 1f);
				if (m_craftingProgress == 1f)
				{
					m_craftingProgress = 0f;
					RemoveTankFluid(0, FluidsManager.GetContainerVolume(slotValue));
					RemoveSlotItems(0, 1);
					AddSlotItems(1, m_container.GetByFirst(slotValue), 1);
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
