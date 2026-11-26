using Engine;
using Game;

namespace SCIENEW.VoltNet;

public class InductionFurnaceVoltElement : VoltElement
{
	public InductionFurnaceVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		return 0f - standardPL;
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		return 0f - standardUL;
	}
}
