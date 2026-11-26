using System.Collections.Generic;
using Engine;
using Game;
using SCIENEW.ProductionIO;
using ZLinq;
using ZLinq.Linq;

namespace SCIENEW;

public class CrafterPopResultsElectricElement : ElectricElement
{
	public bool m_isDispenseAllowed = true;

	public double? m_lastDispenseTime;

	public SubsystemBlockEntities m_subsystemBlockEntities;

	public SubsystemPickables m_subsystemPickables;

	public Vector3 m_position;

	public int m_direction;

	public CrafterPopResultsElectricElement(SubsystemElectricity subsystemElectricity, Point3 point, int direction)
		: base(subsystemElectricity, new List<CellFace>
		{
			new CellFace(point.X, point.Y, point.Z, 0),
			new CellFace(point.X, point.Y, point.Z, 1),
			new CellFace(point.X, point.Y, point.Z, 2),
			new CellFace(point.X, point.Y, point.Z, 3),
			new CellFace(point.X, point.Y, point.Z, 4),
			new CellFace(point.X, point.Y, point.Z, 5)
		})
	{
		m_subsystemBlockEntities = SubsystemElectricity.Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true);
		m_subsystemPickables = SubsystemElectricity.Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_position = new Vector3(point) + new Vector3(0.5f);
		m_direction = direction;
	}

	public override bool Simulate()
	{
		if (CalculateHighInputsCount() > 0)
		{
			if (m_isDispenseAllowed && (!m_lastDispenseTime.HasValue || SubsystemElectricity.SubsystemTime.GameTime - m_lastDispenseTime > 0.1))
			{
				m_isDispenseAllowed = false;
				m_lastDispenseTime = SubsystemElectricity.SubsystemTime.GameTime;
				ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z);
				IInventory inventory = blockEntity?.Entity.FindComponent<IInventory>();
				IInventoryProductionSlots inventoryProductionSlots = (inventory as IInventoryProductionSlots) ?? blockEntity?.Entity.FindComponent<IInventoryProductionSlots>();
				if (inventory != null && inventoryProductionSlots != null)
				{
					Vector3 position = m_position + CellFace.FaceToVector3(m_direction) * 0.7f;
					Vector3 value = CellFace.FaceToVector3(m_direction) * 4f;
					ValueEnumerator<FromEnumerable<int>, int> enumerator = inventoryProductionSlots.GetSlotIndices(ProductionSlotKind.Output).AsValueEnumerable().GetEnumerator<FromEnumerable<int>, int>();
					try
					{
						while (enumerator.MoveNext())
						{
							int current = enumerator.Current;
							if (current >= 0 && current < inventory.SlotsCount)
							{
								int slotValue = inventory.GetSlotValue(current);
								int num = inventory.RemoveSlotItems(current, inventory.GetSlotCount(current));
								if (num > 0)
								{
									m_subsystemPickables.AddPickable(slotValue, num, position, value, null);
								}
							}
						}
					}
					finally
					{
						enumerator.Dispose();
					}
				}
			}
		}
		else
		{
			m_isDispenseAllowed = true;
		}
		return false;
	}
}
