using System;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentGReactorController : Component, IUpdateable
{
	public SubsystemTime m_subsystemTime;

	public SubsystemTerrain m_subsystemTerrain;

	public ComponentBlockEntity m_componentBlockEntity;

	public float m_coreFireTimeRemaining;

	public int m_coreUFuelRodCount;

	public int m_coreCarbonRodCount;

	public int m_coreControlRodCount;

	public int m_sorterUFuelRodCount;

	public int m_sorterCarbonRodCount;

	public int m_sorterControlRodCount;

	public float m_coreHeatLevel;

	public bool fr1;

	public bool cr1;

	public bool gr1;

	public bool fr0;

	public bool cr0;

	public bool gr0;

	public bool az5;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
	}

	public int GetDirection()
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		return FourDirectionModelDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
	}

	private IInventory? GetInventoryIf<T>(Point3 point) where T : DeviceBlock
	{
		if (DevicesUtils.FindDevice(m_subsystemTerrain, point, out var _) is T && BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, point, out ComponentBlockEntity blockEntity))
		{
			return blockEntity.Entity.FindComponent<IInventory>(throwOnError: true);
		}
		return null;
	}

	public void UpdateCoreStatus()
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		Point3 point = CellFace.FaceToPoint3(GetDirection());
		int x = coordinates.X - point.X;
		int num = coordinates.Y - point.Y;
		int z = coordinates.Z - point.Z;
		Point3 point2 = new Point3(x, num, z);
		m_coreUFuelRodCount = 0;
		m_coreControlRodCount = 0;
		m_coreCarbonRodCount = 0;
		m_sorterUFuelRodCount = 0;
		m_sorterControlRodCount = 0;
		m_sorterCarbonRodCount = 0;
		IInventory inventoryIf = GetInventoryIf<GReactorCoreDevice>(point2);
		if (inventoryIf is ComponentGReactorCore componentGReactorCore)
		{
			m_coreHeatLevel = componentGReactorCore.HeatLevel;
			m_coreFireTimeRemaining = componentGReactorCore.m_fireTimeRemaining;
			for (int i = 0; i < inventoryIf.SlotsCount; i++)
			{
				int slotValue = inventoryIf.GetSlotValue(i);
				if (Terrain.ExtractContents(slotValue) == BaseDamageableItemBlock.Index && DamageableItemManager.GetItem(slotValue) is FuelRodDItem fuelRodDItem)
				{
					switch (fuelRodDItem.m_type)
					{
					case FuelRodDItem.RodType.UFuelRod:
						m_coreUFuelRodCount++;
						break;
					case FuelRodDItem.RodType.ControlRod:
						m_coreControlRodCount++;
						break;
					case FuelRodDItem.RodType.CarbonRod:
						m_coreCarbonRodCount++;
						break;
					}
				}
			}
		}
		Point3 point3 = new Point3(x, num + 1, z);
		IInventory inventoryIf2 = GetInventoryIf<GReactorTopDevice>(point3);
		if (!(inventoryIf2 is ComponentGReactorTop))
		{
			return;
		}
		for (int j = 0; j < inventoryIf2.SlotsCount; j++)
		{
			int slotValue2 = inventoryIf2.GetSlotValue(j);
			if (Terrain.ExtractContents(slotValue2) == BaseDamageableItemBlock.Index && DamageableItemManager.GetItem(slotValue2) is FuelRodDItem fuelRodDItem2)
			{
				switch (fuelRodDItem2.m_type)
				{
				case FuelRodDItem.RodType.UFuelRod:
					m_sorterUFuelRodCount++;
					break;
				case FuelRodDItem.RodType.ControlRod:
					m_sorterControlRodCount++;
					break;
				case FuelRodDItem.RodType.CarbonRod:
					m_sorterCarbonRodCount++;
					break;
				}
			}
		}
	}

	public void TryInsertRod()
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		Point3 point = CellFace.FaceToPoint3(GetDirection());
		int x = coordinates.X - point.X;
		int num = coordinates.Y - point.Y;
		int z = coordinates.Z - point.Z;
		Point3 point2 = new Point3(x, num, z);
		bool flag = false;
		IInventory inventoryIf = GetInventoryIf<GReactorCoreDevice>(point2);
		if (inventoryIf is ComponentGReactorCore componentGReactorCore)
		{
			m_coreHeatLevel = componentGReactorCore.HeatLevel;
			m_coreFireTimeRemaining = componentGReactorCore.m_fireTimeRemaining;
			for (int i = 0; i < inventoryIf.SlotsCount; i++)
			{
				if (inventoryIf.GetSlotValue(i) == 0)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			fr1 = false;
			cr1 = false;
			gr1 = false;
			return;
		}
		Point3 point3 = new Point3(x, num + 1, z);
		int num2 = 0;
		IInventory inventoryIf2 = GetInventoryIf<GReactorTopDevice>(point3);
		if (inventoryIf2 is ComponentGReactorTop)
		{
			for (int j = 0; j < inventoryIf2.SlotsCount; j++)
			{
				int slotValue = inventoryIf2.GetSlotValue(j);
				if (Terrain.ExtractContents(slotValue) == BaseDamageableItemBlock.Index && DamageableItemManager.GetItem(slotValue) is FuelRodDItem fuelRodDItem)
				{
					if (fuelRodDItem.m_type == FuelRodDItem.RodType.UFuelRod && fr1)
					{
						num2 = slotValue;
						inventoryIf2.RemoveSlotItems(j, 1);
						break;
					}
					if (fuelRodDItem.m_type == FuelRodDItem.RodType.ControlRod && cr1)
					{
						num2 = slotValue;
						inventoryIf2.RemoveSlotItems(j, 1);
						break;
					}
					if (fuelRodDItem.m_type == FuelRodDItem.RodType.CarbonRod && gr1)
					{
						num2 = slotValue;
						inventoryIf2.RemoveSlotItems(j, 1);
						break;
					}
				}
			}
		}
		if (num2 == 0)
		{
			return;
		}
		IInventory inventoryIf3 = GetInventoryIf<GReactorCoreDevice>(point2);
		if (inventoryIf3 is ComponentGReactorCore)
		{
			for (int k = 0; k < inventoryIf3.SlotsCount; k++)
			{
				if (inventoryIf3.GetSlotValue(k) == 0)
				{
					inventoryIf3.AddSlotItems(k, num2, 1);
					break;
				}
			}
		}
		fr1 = false;
		cr1 = false;
		gr1 = false;
	}

	public void TryExtractRod()
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		Point3 point = CellFace.FaceToPoint3(GetDirection());
		int x = coordinates.X - point.X;
		int num = coordinates.Y - point.Y;
		int z = coordinates.Z - point.Z;
		Point3 point2 = new Point3(x, num + 1, z);
		Point3 point3 = new Point3(x, num, z);
		bool flag = false;
		IInventory inventoryIf = GetInventoryIf<GReactorTopDevice>(point2);
		if (inventoryIf is ComponentGReactorTop)
		{
			for (int i = 0; i < inventoryIf.SlotsCount; i++)
			{
				if (inventoryIf.GetSlotValue(i) == 0)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			fr0 = false;
			cr0 = false;
			gr0 = false;
			return;
		}
		int num2 = 0;
		IInventory inventoryIf2 = GetInventoryIf<GReactorCoreDevice>(point3);
		if (inventoryIf2 is ComponentGReactorCore)
		{
			for (int j = 0; j < inventoryIf2.SlotsCount; j++)
			{
				int slotValue = inventoryIf2.GetSlotValue(j);
				if (Terrain.ExtractContents(slotValue) == BaseDamageableItemBlock.Index && DamageableItemManager.GetItem(slotValue) is FuelRodDItem fuelRodDItem)
				{
					if (fuelRodDItem.m_type == FuelRodDItem.RodType.UFuelRod && fr0)
					{
						num2 = slotValue;
						inventoryIf2.RemoveSlotItems(j, 1);
						break;
					}
					if (fuelRodDItem.m_type == FuelRodDItem.RodType.ControlRod && cr0)
					{
						num2 = slotValue;
						inventoryIf2.RemoveSlotItems(j, 1);
						break;
					}
					if (fuelRodDItem.m_type == FuelRodDItem.RodType.CarbonRod && gr0)
					{
						num2 = slotValue;
						inventoryIf2.RemoveSlotItems(j, 1);
						break;
					}
				}
			}
		}
		if (num2 == 0)
		{
			return;
		}
		IInventory inventoryIf3 = GetInventoryIf<GReactorTopDevice>(point2);
		if (inventoryIf3 is ComponentGReactorTop)
		{
			for (int k = 0; k < inventoryIf3.SlotsCount; k++)
			{
				if (inventoryIf3.GetSlotValue(k) == 0)
				{
					inventoryIf3.AddSlotItems(k, num2, 1);
					break;
				}
			}
		}
		fr0 = false;
		cr0 = false;
		gr0 = false;
	}

	public void Update(float dt)
	{
		if (m_subsystemTime.PeriodicGameTimeEvent(0.2, (double)GetHashCode() % 1000.0 / 1000.0))
		{
			UpdateCoreStatus();
		}
		if (fr1 || cr1 || gr1)
		{
			TryInsertRod();
		}
		if (fr0 || cr0 || gr0)
		{
			TryExtractRod();
		}
		if (az5 && m_sorterControlRodCount > 0)
		{
			cr1 = true;
		}
		else
		{
			az5 = false;
		}
	}

	public string GetStateText()
	{
		string text = LanguageUtils.GetText(this, "StateText");
		_003C_003Ey__InlineArray8<object> buffer = default;
		buffer[0] = m_coreHeatLevel.ToString("F2");
		buffer[1] = m_coreFireTimeRemaining;
		buffer[2] = m_coreUFuelRodCount;
		buffer[3] = m_coreControlRodCount;
		buffer[4] = m_coreCarbonRodCount;
		buffer[5] = m_sorterUFuelRodCount;
		buffer[6] = m_sorterControlRodCount;
		buffer[7] = m_sorterCarbonRodCount;
		return string.Format(text, (ReadOnlySpan<object?>)buffer);
	}
}
