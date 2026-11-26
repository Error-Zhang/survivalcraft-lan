using Engine;
using Game;

namespace SCIENEW;

public class HearthFurnaceOutputPortTransferrer : FluidTransferrer
{
	private IHearthFurnacePortHost? m_host;

	public HearthFurnaceOutputPortTransferrer(Point3 position, SubsystemTerrain subsystemTerrain)
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

	public override int GetFluidValue(ITank tank)
	{
		return tank.GetTankValue(1);
	}

	public override int RemoveFluid(ITank tank, float volume, out float removedVolume, out int removedTankIndex)
	{
		if (m_host == null)
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		int moltenSteelTankIndex = m_host.MoltenSteelTankIndex;
		if (tank.GetTankVolume(moltenSteelTankIndex) <= 0f)
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		if (!(tank is IHearthFurnacePortHost hearthFurnacePortHost))
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		if (!hearthFurnacePortHost.IsOutputOpen)
		{
			return base.RemoveFluid(tank, volume, out removedVolume, out removedTankIndex);
		}
		int tankValue = tank.GetTankValue(moltenSteelTankIndex);
		removedVolume = tank.RemoveTankFluid(moltenSteelTankIndex, volume);
		removedTankIndex = moltenSteelTankIndex;
		return tankValue;
	}
}
