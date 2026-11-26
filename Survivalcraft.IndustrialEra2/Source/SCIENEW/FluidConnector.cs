using Game;

namespace SCIENEW;

public class FluidConnector
{
	public CellFace MountedFace;

	public FluidConnectorType ConnectorType;

	public float Rate;

	public FluidConnector(CellFace mountedFace, FluidConnectorType type, float rate)
	{
		MountedFace = mountedFace;
		ConnectorType = type;
		Rate = rate;
	}
}
