using Engine;
using Game;

namespace SCIENEW;

public class FractionalTowerPortTransferrer : FluidTransferrer
{
	private IFractionalTowerPortHost? m_host;

	private int? m_tankIndex;

	public FractionalTowerPortTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
		: base(position, subsystemTerrain)
	{
	}

	public void BindHost(IFractionalTowerPortHost? host, int tankIndex)
	{
		m_host = host;
		m_tankIndex = ((host != null) ? new int?(tankIndex) : ((int?)null));
	}

	public void ResetHost()
	{
		m_host = null;
		m_tankIndex = null;
	}

	public override ITank? GetTank()
	{
		return m_host;
	}

	public override int GetFluidValue(ITank tank)
	{
		if (!m_tankIndex.HasValue)
		{
			return 0;
		}
		return tank.GetTankValue(m_tankIndex.Value);
	}

	public override int RemoveFluid(ITank tank, float volume, out float removedVolume, out int removedTankIndex)
	{
		if (!m_tankIndex.HasValue)
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		if (tank.GetTankVolume(m_tankIndex.Value) <= 0f)
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		if (!(tank is IFractionalTowerPortHost))
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		int tankValue = tank.GetTankValue(m_tankIndex.Value);
		removedVolume = tank.RemoveTankFluid(m_tankIndex.Value, volume);
		removedTankIndex = m_tankIndex.Value;
		return tankValue;
	}
}
