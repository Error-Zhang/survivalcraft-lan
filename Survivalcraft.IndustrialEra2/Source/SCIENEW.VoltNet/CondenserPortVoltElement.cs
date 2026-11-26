using Engine;
using Game;

namespace SCIENEW.VoltNet;

public class CondenserPortVoltElement : VoltElement
{
	public CondenserPortRole m_role;

	private double m_hostCacheTime = double.NegativeInfinity;

	private ComponentCondenser? m_host;

	public CondenserPortVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue, CondenserPortRole role)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_role = role;
	}

	private ComponentCondenser? GetHost()
	{
		double gameTime = SubsystemVoltNet.m_subsystemTime.GameTime;
		if (m_hostCacheTime != gameTime)
		{
			m_hostCacheTime = gameTime;
			CondenserPortUtils.TryGetHostCondenser(SubsystemTerrain, Position, BlockValue, m_role, out m_host);
		}
		return m_host;
	}

	public override void Update(float dt, float inputVoltage, float inputPower)
	{
		ComponentCondenser host = GetHost();
		if (host == null)
		{
			return;
		}
		if (m_role == CondenserPortRole.Input)
		{
			bool flag = inputPower != 0f && inputVoltage != 0f;
			if (flag)
			{
				WorkState currentWorkState = CurrentWorkState;
				bool flag2 = ((currentWorkState == WorkState.Broken || currentWorkState == WorkState.Overloaded) ? true : false);
				flag = !flag2;
			}
			if (flag)
			{
				host.InputEnergy(inputPower * dt);
			}
		}
		else if (host.HasEnergy)
		{
			int num = CalcConnection();
			if (num > 0)
			{
				host.OutputEnergy(host.OutputPower * dt * (float)num);
			}
		}
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		ComponentCondenser host = GetHost();
		if (host == null)
		{
			return 0f;
		}
		if (m_role == CondenserPortRole.Input)
		{
			Point3 hostPosition = CondenserPortUtils.GetHostPosition(BlockValue, Position);
			if (!CondenserPortUtils.TryGetHostVoltRatings(SubsystemTerrain, hostPosition, out var standardPL2, out var _))
			{
				return 0f;
			}
			return 0f - standardPL2;
		}
		if (host.HasEnergy)
		{
			return host.OutputPower;
		}
		return 0f;
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		ComponentCondenser host = GetHost();
		if (host == null)
		{
			return 0f;
		}
		if (m_role == CondenserPortRole.Input)
		{
			Point3 hostPosition = CondenserPortUtils.GetHostPosition(BlockValue, Position);
			if (!CondenserPortUtils.TryGetHostVoltRatings(SubsystemTerrain, hostPosition, out var _, out var standardUL2))
			{
				return 0f;
			}
			return 0f - standardUL2;
		}
		if (host.HasEnergy)
		{
			return host.OutputVoltage;
		}
		return 0f;
	}

	public override void UpdateWorkState(float inputPower, float inputVoltage, float requiredPower, float requiredVoltage, float tolerance = 0.05f)
	{
		if (GetHost() == null)
		{
			CurrentWorkState = WorkState.Broken;
		}
		else if (m_role == CondenserPortRole.Input)
		{
			Point3 hostPosition = CondenserPortUtils.GetHostPosition(BlockValue, Position);
			if (!CondenserPortUtils.TryGetHostVoltRatings(SubsystemTerrain, hostPosition, out var standardPL, out var standardUL))
			{
				CurrentWorkState = WorkState.Broken;
			}
			else
			{
				base.UpdateWorkState(inputPower, inputVoltage, standardPL, standardUL, tolerance);
			}
		}
		else
		{
			base.UpdateWorkState(inputPower, inputVoltage, requiredPower, requiredVoltage, tolerance);
		}
	}
}
