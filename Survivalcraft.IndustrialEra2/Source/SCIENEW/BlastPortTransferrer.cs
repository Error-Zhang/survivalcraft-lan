using Engine;
using Game;

namespace SCIENEW;

public class BlastPortTransferrer : FluidTransferrer
{
	private IBlastFurnacePortHost? m_host;

	public BlastPortTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public void BindHost(IBlastFurnacePortHost? host)
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

	public override int RemoveFluid(ITank tank, float volume, out float removedVolume, out int removedTankIndex)
	{
		if (tank.GetTankVolume(0) <= 0f)
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		if (!(tank is IBlastFurnacePortHost blastFurnacePortHost))
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		if (!blastFurnacePortHost.IsBlastPortOpen)
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		int tankValue = tank.GetTankValue(0);
		removedVolume = tank.RemoveTankFluid(0, volume);
		removedTankIndex = 0;
		return tankValue;
	}
}
