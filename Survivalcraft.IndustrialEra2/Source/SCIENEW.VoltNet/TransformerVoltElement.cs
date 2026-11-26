using System;
using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW.VoltNet;

public class TransformerVoltElement : VoltElement
{
	public int m_inputFace;

	public float m_voltDelta;

	public float m_eta;

	protected float m_inputVoltage;

	protected float m_inputPower;

	public TransformerVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue, float voltDelta, float eta)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_inputFace = SixDirectionModelDevice.GetDirection_Static(blockValue);
		m_voltDelta = voltDelta;
		m_eta = eta;
	}

	public override void Update(float dt, float inputVoltage, float inputPower)
	{
		base.Update(dt, inputVoltage, inputPower);
		m_inputVoltage = inputVoltage;
		m_inputPower = inputPower;
	}

	public float GetOutputConsumerDemand()
	{
		float num = 0f;
		foreach (VoltConnection belongingConnection in m_belongingConnections)
		{
			foreach (KeyValuePair<CellFace, VoltElement> containsElement in belongingConnection.m_containsElements)
			{
				if (containsElement.Value == this && containsElement.Key.Face != m_inputFace)
				{
					num += belongingConnection.GetConsumerDemand();
					break;
				}
			}
		}
		return num;
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		if (cellFace.Face == m_inputFace)
		{
			return 0f - GetConsumerDemand(standardPL, cellFace);
		}
		return m_inputPower * m_eta;
	}

	public override float GetConsumerDemand(float standardPL, CellFace cellFace)
	{
		if (cellFace.Face != m_inputFace || m_eta <= 0f)
		{
			return 0f;
		}
		return MathUtils.Min(GetOutputConsumerDemand() / m_eta, MathF.Abs(standardPL));
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		if (cellFace.Face != m_inputFace)
		{
			return m_inputVoltage * m_voltDelta;
		}
		return base.GetUL(standardUL, cellFace);
	}
}
