using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class TransferElectricElement : ElectricElement
{
	public bool m_isDispenseAllowed = true;

	public double? m_lastDispenseTime;

	public SubsystemBlockEntities m_subsystemBlockEntities;

	public SubsystemTerrain m_subsystemTerrain;

	public TransferElectricElement(SubsystemElectricity subsystemElectricity, Point3 point)
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
		m_subsystemTerrain = SubsystemElectricity.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	public override bool Simulate()
	{
		if (CalculateHighInputsCount() > 0 && (!m_lastDispenseTime.HasValue || SubsystemElectricity.SubsystemTime.GameTime - m_lastDispenseTime > 0.1))
		{
			int innerData = DevicesUtils.GetInnerData(m_subsystemTerrain.Terrain.GetCellValue(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z));
			Vector3 vector = CellFace.FaceToVector3(innerData);
			Vector3 vector2 = -CellFace.FaceToVector3(innerData);
			ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X + (int)vector.X, CellFaces[0].Point.Y + (int)vector.Y, CellFaces[0].Point.Z + (int)vector.Z);
			ComponentBlockEntity blockEntity2 = m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X + (int)vector2.X, CellFaces[0].Point.Y + (int)vector2.Y, CellFaces[0].Point.Z + (int)vector2.Z);
			bool flag = false;
			if (blockEntity != null && blockEntity2 != null)
			{
				ComponentInventoryBase componentInventoryBase = blockEntity.Entity.FindComponent<ComponentInventoryBase>();
				ComponentInventoryBase componentInventoryBase2 = blockEntity2.Entity.FindComponent<ComponentInventoryBase>();
				if (componentInventoryBase != null && componentInventoryBase2 != null && componentInventoryBase.SlotsCount >= componentInventoryBase2.SlotsCount)
				{
					for (int i = 0; i < componentInventoryBase2.SlotsCount; i++)
					{
						if (componentInventoryBase2.m_slots[i].Count != 0 && (componentInventoryBase.m_slots[i].Count == 0 || (componentInventoryBase.m_slots[i].Value == componentInventoryBase2.m_slots[i].Value && componentInventoryBase.m_slots[i].Count < componentInventoryBase.GetSlotCapacity(i, componentInventoryBase.m_slots[i].Value))))
						{
							componentInventoryBase.AddSlotItems(i, componentInventoryBase2.m_slots[i].Value, componentInventoryBase2.m_slots[i].Count);
							componentInventoryBase2.RemoveSlotItems(i, componentInventoryBase2.m_slots[i].Count);
						}
					}
				}
			}
			if (flag)
			{
				m_lastDispenseTime = SubsystemElectricity.SubsystemTime.GameTime;
			}
		}
		return false;
	}
}
