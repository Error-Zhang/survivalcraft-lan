using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.ProductionIO;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentFluidInserter : ComponentTankBase
{
	public int m_inIndex;

	public int m_outIndex;

	public SubsystemTerrain m_subsystemTerrain;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemFluidTransfer m_subsystemFluidTransfer;

	public float m_unitVolume = 4f;

	public bool Insert()
	{
		if (!TryResolveTanks(out ITank fromTank, out ITank toTank))
		{
			return false;
		}
		int filterValue = ((GetTankVolume(0) > 0f) ? GetTankValue(0) : 0);
		return TryInsertBetweenTanks(fromTank, toTank, m_inIndex, m_outIndex, filterValue, m_unitVolume);
	}

	private bool TryResolveTanks(out ITank fromTank, out ITank toTank)
	{
		fromTank = null;
		toTank = null;
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int direction_Static = SixDirectionModelDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
		int face = CellFace.OppositeFace(direction_Static);
		int face2 = direction_Static;
		Point3 point = m_componentBlockEntity.Coordinates + CellFace.FaceToPoint3(face);
		Point3 point2 = m_componentBlockEntity.Coordinates + CellFace.FaceToPoint3(face2);
		if (m_subsystemFluidTransfer.TryGetTransferrer(point, out FluidTransferrer transferrer))
		{
			fromTank = transferrer.GetTank();
			if (fromTank == null)
			{
				return false;
			}
		}
		else
		{
			if (!BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, point, out ComponentBlockEntity blockEntity))
			{
				return false;
			}
			fromTank = blockEntity.Entity.FindComponent<ITank>();
			if (fromTank == null)
			{
				return false;
			}
		}
		if (m_subsystemFluidTransfer.TryGetTransferrer(point2, out FluidTransferrer transferrer2))
		{
			toTank = transferrer2.GetTank();
			if (toTank == null)
			{
				return false;
			}
		}
		else
		{
			if (!BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, point2, out ComponentBlockEntity blockEntity2))
			{
				return false;
			}
			toTank = blockEntity2.Entity.FindComponent<ITank>();
			if (toTank == null)
			{
				return false;
			}
		}
		return true;
	}

	private static bool TryInsertBetweenTanks(ITank fromTank, ITank toTank, int inIndex, int outIndex, int filterValue, float unitVolume)
	{
		if (inIndex > fromTank.TanksCount || outIndex > toTank.TanksCount)
		{
			return false;
		}
		foreach (int item in ProductionSlotAccess.GetTankSourceSlotOrder(fromTank, filterValue, inIndex))
		{
			int tankValue = fromTank.GetTankValue(item);
			if (tankValue == 0 || (filterValue != 0 && tankValue != filterValue) || !ProductionSlotAccess.CanInsertFluidIntoInputTanks(toTank, tankValue, outIndex))
			{
				continue;
			}
			float num = fromTank.RemoveTankFluid(item, unitVolume);
			if (!(num <= 0f))
			{
				float num2 = ProductionSlotAccess.InsertFluidIntoInputTanks(toTank, tankValue, num, outIndex);
				fromTank.AddTankFluid(item, tankValue, num2, out var _);
				if (num2 < num)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemFluidTransfer = Project.FindSubsystem<SubsystemFluidTransfer>(throwOnError: true);
		m_inIndex = valuesDictionary.GetValue("InIndex", 0);
		m_outIndex = valuesDictionary.GetValue("OutIndex", 0);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("InIndex", m_inIndex);
		valuesDictionary.SetValue("OutIndex", m_outIndex);
	}
}
