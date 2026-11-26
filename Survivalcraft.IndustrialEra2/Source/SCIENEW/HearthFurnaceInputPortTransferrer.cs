using Engine;
using Game;

namespace SCIENEW;

public class HearthFurnaceInputPortTransferrer : FluidTransferrer
{
	private IHearthFurnacePortHost? m_host;

	public HearthFurnaceInputPortTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public void BindHost(IHearthFurnacePortHost? host)
	{
		m_host = host;
	}

	public void ResetHost()
	{
		m_host = null;
	}

	public override ITank? GetTank()
	{
		return m_host;
	}

	public override bool IsFull(ITank tank)
	{
		if (!base.IsFull(tank))
		{
			IHearthFurnacePortHost host = m_host;
			if (host != null)
			{
				return !host.IsInputOpen;
			}
			return false;
		}
		return true;
	}

	public override float AcquireFluid(ITank tank, float volume, int fluidValue, CellFace connectorFace)
	{
		IHearthFurnacePortHost host = m_host;
		if (host == null || !host.IsInputOpen)
		{
			return 0f;
		}
		return ComponentInventoryTankBase.AcquireFluid(tank, fluidValue, volume);
	}
}
