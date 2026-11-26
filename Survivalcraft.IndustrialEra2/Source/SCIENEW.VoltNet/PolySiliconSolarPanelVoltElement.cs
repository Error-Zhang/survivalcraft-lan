using Engine;
using Game;

namespace SCIENEW.VoltNet;

public class PolySiliconSolarPanelVoltElement : SolarPanelVoltElement
{
	public PolySiliconSolarPanelVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
	}
}
