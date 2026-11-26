using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX.ComponentsExtra.Implementation;
using SCIENEW.ProductionIO;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentInserter : ComponentInventoryBase
{
	protected ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemBlockEntities m_subsystemBlockEntities;

	public SubsystemPickables m_subsystemPickables;

	public int m_inNum;

	public int m_outNum;

	public bool m_dispenseItem = true;

	public bool Place()
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int cellValue = m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z);
		int num = ((GetSlotCount(0) > 0) ? GetSlotValue(0) : 0);
		Vector3 vector = CellFace.FaceToVector3(DevicesUtils.GetSixFaceDirection(cellValue));
		Vector3 dropPosition = new Vector3(coordinates) + new Vector3(0.5f) + 0.6f * vector;
		Point3 point = coordinates - new Point3((int)vector.X, (int)vector.Y, (int)vector.Z);
		Point3 destCoords = coordinates + new Point3((int)vector.X, (int)vector.Y, (int)vector.Z);
		ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(point.X, point.Y, point.Z);
		IInventory inventory = blockEntity?.Entity.FindComponent<IInventory>();
		if (inventory == null)
		{
			return false;
		}
		foreach (int item in ProductionSlotAccess.GetInventorySourceSlotOrder(blockEntity.Entity, inventory, num, m_inNum))
		{
			if (inventory.GetSlotCount(item) <= 0)
			{
				continue;
			}
			int transferCount = GetTransferCount(blockEntity.Entity, item, inventory);
			if (transferCount > 0)
			{
				int slotValue = inventory.GetSlotValue(item);
				if ((num == 0 || slotValue == num) && TryTransferFromSlot(blockEntity.Entity, inventory, item, slotValue, transferCount, destCoords, dropPosition, vector))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static int GetTransferCount(Entity sourceEntity, int slotIndex, IInventory sourceInventory)
	{
		ComponentEXCraftingTable componentEXCraftingTable = sourceEntity.FindComponent<ComponentEXCraftingTable>();
		if (componentEXCraftingTable == null || slotIndex != componentEXCraftingTable.ResultSlotIndex)
		{
			return 1;
		}
		if (componentEXCraftingTable.m_matchedRecipe == null)
		{
			return 0;
		}
		return MathUtils.Min(componentEXCraftingTable.m_matchedRecipe.ResultCount, sourceInventory.GetSlotCount(slotIndex));
	}

	private bool TryTransferFromSlot(Entity sourceEntity, IInventory sourceInventory, int slotIndex, int itemValue, int itemCount, Point3 destCoords, Vector3 dropPosition, Vector3 faceVector)
	{
		ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(destCoords.X, destCoords.Y, destCoords.Z);
		IInventory inventory = blockEntity?.Entity.FindComponent<IInventory>();
		if (inventory != null)
		{
			if (ProductionSlotAccess.TryInsertIntoInputSlots(blockEntity.Entity, inventory, itemValue, itemCount, m_outNum))
			{
				sourceInventory.RemoveSlotItems(slotIndex, itemCount);
				return true;
			}
		}
		else if (m_dispenseItem)
		{
			m_subsystemPickables.AddPickable(itemValue, itemCount, dropPosition, 1.8f * (faceVector + m_random.Vector3(0.2f)), null);
			sourceInventory.RemoveSlotItems(slotIndex, itemCount);
			return true;
		}
		return false;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemBlockEntities = Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true);
		m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_inNum = valuesDictionary.GetValue("Innum", 0);
		m_outNum = valuesDictionary.GetValue("Outnum", 0);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("Innum", m_inNum);
		valuesDictionary.SetValue("Outnum", m_outNum);
	}
}
