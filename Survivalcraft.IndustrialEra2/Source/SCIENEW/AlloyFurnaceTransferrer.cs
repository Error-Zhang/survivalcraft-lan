using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class AlloyFurnaceTransferrer : FluidTransferrer
{
	private Dictionary<int, int> m_faceToTank = new Dictionary<int, int>();

	public AlloyFurnaceTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
		int direction_Static = IlluminableDevice.GetDirection_Static(subsystemTerrain.Terrain.GetCellValueFast(position.X, position.Y, position.Z));
		m_faceToTank.Add(DevicesUtils.GetLeftFace(direction_Static), 0);
		m_faceToTank.Add(CellFace.OppositeFace(direction_Static), 1);
		m_faceToTank.Add(DevicesUtils.GetRightFace(direction_Static), 2);
	}

	public override bool IsFull(ITank tank)
	{
		bool flag = true;
		for (int i = 0; i < 3; i++)
		{
			flag &= tank.GetTankVolume(i) >= tank.GetTankOriginalCapacity(i);
		}
		return flag;
	}

	public override float AcquireFluid(ITank tank, float volume, int fluidValue, CellFace connectorFace)
	{
		return ComponentInventoryTankBase.AcquireFluid(tank, fluidValue, volume, m_faceToTank[connectorFace.Face]..(m_faceToTank[connectorFace.Face] + 1));
	}
}
