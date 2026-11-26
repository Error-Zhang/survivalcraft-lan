using Engine;
using Game;

namespace SCIENEW.VoltNet;

public class VoltmeterVoltElement : VoltElement
{
	public VoltmeterVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
	}

	public override void UpdateWorkState(float inputPower, float inputVoltage, float requiredPower, float requiredVoltage, float tolerance = 0.05f)
	{
		CurrentWorkState = WorkState.Active;
	}
}
