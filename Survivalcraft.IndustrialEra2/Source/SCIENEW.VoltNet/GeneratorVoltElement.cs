using Engine;
using Game;

namespace SCIENEW.VoltNet;

public class GeneratorVoltElement : VoltElement
{
	public SubsystemEnginePower m_subsystemEnginePower;

	private CellFace m_inputCellFace;

	public GeneratorVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_subsystemEnginePower = subsystemVoltNet.Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		int face = CellFace.OppositeFace(SixDirectionModelDevice.GetDirection_Static(BlockValue));
		m_inputCellFace = new CellFace(position.X, position.Y, position.Z, face);
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		if (!m_subsystemEnginePower.IsPowered(m_inputCellFace, out IEngineComponent _))
		{
			return 0f;
		}
		return standardPL;
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		if (!m_subsystemEnginePower.IsPowered(m_inputCellFace, out IEngineComponent _))
		{
			return 0f;
		}
		return standardUL;
	}
}
