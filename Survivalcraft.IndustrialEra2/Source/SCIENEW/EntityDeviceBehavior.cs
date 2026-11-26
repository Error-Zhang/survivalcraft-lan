using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW;

public class EntityDeviceBehavior<T> : DeviceBehavior where T : Component
{
	protected SubsystemBlockEntities m_subsystemBlockEntities;

	protected SubsystemAudio m_subsystemAudio;

	public EntityDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemAudio = project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		m_subsystemBlockEntities = project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true);
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		if (DevicesBlockManager.GetDevice(value) is IEntityDevice<T> entityDevice)
		{
			string entityName = entityDevice.GetEntityName(value);
			BlockEntityUtils.CreateBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), entityName, new Point3(x, y, z));
		}
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		BlockEntityUtils.RemoveBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), new Point3(x, y, z));
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		DeviceBlock device = DevicesBlockManager.GetDevice(oldValue);
		DeviceBlock device2 = DevicesBlockManager.GetDevice(value);
		ComponentBlockEntity blockEntity;
		if (device.GetCraftingId() != device2.GetCraftingId())
		{
			BlockEntityUtils.RemoveBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), new Point3(x, y, z));
			if (device2 is IEntityDevice<T> entityDevice)
			{
				string entityName = entityDevice.GetEntityName(value);
				BlockEntityUtils.CreateBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), entityName, new Point3(x, y, z));
			}
		}
		else if (BlockEntityUtils.GetBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), new Point3(x, y, z), out blockEntity))
		{
			blockEntity.Entity.FindComponent<IRotatableDeviceComponent>()?.OnRotated(oldValue, value);
		}
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		if (DevicesBlockManager.GetDevice(raycastResult.Value) is IEntityDevice<T> entityDevice && BlockEntityUtils.GetBlockEntity(Project.FindSubsystem<SubsystemTerrain>(throwOnError: true), raycastResult.CellFace.Point, out ComponentBlockEntity blockEntity) && componentMiner.ComponentPlayer != null)
		{
			T component = blockEntity.Entity.FindComponent<T>(throwOnError: true);
			componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = entityDevice.GetWidget(componentMiner.Inventory, component);
			AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
			return true;
		}
		return false;
	}

	public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
	{
		if (worldItem.ToRemove)
		{
			return;
		}
		ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(cellFace.X, cellFace.Y, cellFace.Z);
		if (blockEntity == null)
		{
			return;
		}
		foreach (IInventory item in (IEnumerable<IInventory>)blockEntity.Entity.FindComponents<IInventory>())
		{
			AcquireItem(item, cellFace, worldItem);
		}
	}

	public virtual void AcquireItem(IInventory inventory, CellFace cellFace, WorldItem worldItem)
	{
		if (inventory == null || worldItem.ToRemove)
		{
			return;
		}
		Pickable pickable = worldItem as Pickable;
		int value = worldItem.Value;
		int num = pickable?.Count ?? 1;
		if (inventory is IElectricElementCrafterComponent { RequireItemSlotIndex: not null, RequireItemSlotIndex: var requireItemSlotIndex })
		{
			int value2 = requireItemSlotIndex.Value;
			int num2 = MathUtils.Min(num, inventory.GetSlotCapacity(value2, value) - inventory.GetSlotCount(value2));
			if (inventory.GetSlotCount(value2) == 0 || inventory.GetSlotValue(value2) == value)
			{
				inventory.AddSlotItems(value2, value, num2);
				if (num2 > 0)
				{
					m_subsystemAudio.PlaySound("Audio/PickableCollected", 1f, 0f, worldItem.Position, 3f, autoDelay: true);
				}
				if (num <= num2)
				{
					worldItem.ToRemove = true;
				}
				else if (pickable != null)
				{
					pickable.Count = num - num2;
				}
			}
		}
		else
		{
			int num3 = ComponentInventoryBase.AcquireItems(inventory, worldItem.Value, num);
			if (num3 < num)
			{
				m_subsystemAudio.PlaySound("Audio/PickableCollected", 1f, 0f, worldItem.Position, 3f, autoDelay: true);
			}
			if (num3 <= 0)
			{
				worldItem.ToRemove = true;
			}
			else if (pickable != null)
			{
				pickable.Count = num3;
			}
		}
	}
}
