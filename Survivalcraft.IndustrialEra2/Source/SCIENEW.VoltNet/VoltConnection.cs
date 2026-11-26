using System;
using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW.VoltNet;

public class VoltConnection
{
	public readonly Dictionary<CellFace, VoltElement> m_containsElements;

	public Dictionary<VoltElement, (float U, float P)> m_outputToElements = new Dictionary<VoltElement, (float, float)>();

	protected float m_totalInputPower;

	protected float m_inputVoltage;

	public SubsystemVoltNet SubsystemVoltNet;

	public float InputPower => m_totalInputPower;

	public float InputVoltage => m_inputVoltage;

	public VoltConnection(Dictionary<CellFace, VoltElement> containsElements, SubsystemVoltNet subsystemVoltNet)
	{
		m_containsElements = containsElements;
		SubsystemVoltNet = subsystemVoltNet;
		foreach (KeyValuePair<CellFace, VoltElement> containsElement in containsElements)
		{
			m_outputToElements.Add(containsElement.Value, (0f, 0f));
			containsElement.Value.m_belongingConnections.Add(this);
		}
	}

	public Vector3 CalcCenterPos()
	{
		Vector3 zero = Vector3.Zero;
		foreach (KeyValuePair<CellFace, VoltElement> containsElement in m_containsElements)
		{
			Vector3 vector = new Vector3(containsElement.Key.Point) + new Vector3(0.5f) + CellFace.FaceToVector3(containsElement.Key.Face) * 0.4f;
			zero += vector;
		}
		return zero / m_containsElements.Count;
	}

	public virtual void Simulate(float dt)
	{
		m_totalInputPower = 0f;
		m_inputVoltage = 0f;
		int num = 0;
		foreach (KeyValuePair<CellFace, VoltElement> containsElement in m_containsElements)
		{
			if (containsElement.Value == null)
			{
				continue;
			}
			m_outputToElements[containsElement.Value] = (0f, 0f);
			int blockValue = containsElement.Value.BlockValue;
			if (IsGenerator(containsElement.Value, containsElement.Key))
			{
				float elementUL = GetElementUL(blockValue, containsElement.Value, containsElement.Key);
				if (num == 0)
				{
					m_inputVoltage = elementUL;
				}
				else if (elementUL != m_inputVoltage)
				{
					HandleVoltageMismatch();
					return;
				}
				m_totalInputPower += GetElementPL(blockValue, containsElement.Value, containsElement.Key);
				num++;
			}
		}
		if (num == 0)
		{
			return;
		}
		float consumerDemand = GetConsumerDemand();
		if (consumerDemand == 0f)
		{
			return;
		}
		foreach (KeyValuePair<CellFace, VoltElement> containsElement2 in m_containsElements)
		{
			if (containsElement2.Value != null && IsConsumer(containsElement2.Value, containsElement2.Key))
			{
				int blockValue2 = containsElement2.Value.BlockValue;
				float elementConsumerDemand = GetElementConsumerDemand(blockValue2, containsElement2.Value, containsElement2.Key);
				float item = ((consumerDemand > 0f) ? (m_totalInputPower * (elementConsumerDemand / consumerDemand)) : 0f);
				m_outputToElements[containsElement2.Value] = (m_inputVoltage, item);
			}
		}
	}

	private bool IsGenerator(VoltElement element, CellFace cellFace)
	{
		return GetElementPL(element.BlockValue, element, cellFace) > 0f;
	}

	private bool IsConsumer(VoltElement element, CellFace cellFace)
	{
		return GetElementConsumerDemand(element.BlockValue, element, cellFace) > 0f;
	}

	public bool HasConsumerDemand()
	{
		return GetConsumerDemand() > 0f;
	}

	public float GetConsumerDemand()
	{
		float num = 0f;
		foreach (KeyValuePair<CellFace, VoltElement> containsElement in m_containsElements)
		{
			if (containsElement.Value != null)
			{
				int blockValue = containsElement.Value.BlockValue;
				num += GetElementConsumerDemand(blockValue, containsElement.Value, containsElement.Key);
			}
		}
		return num;
	}

	protected virtual void HandleVoltageMismatch()
	{
		SubsystemVoltNet.PlayVoltageArc(CalcCenterPos());
		foreach (VoltElement value in m_containsElements.Values)
		{
			value.ApplyConnectionVoltageFault();
		}
		SubsystemVoltNet.QueueRemoveDirectConnection(this);
	}

	public float GetElementPL(int blockValue, VoltElement voltElement, CellFace cellFace)
	{
		IVoltDevice voltDevice = VoltElement.ResolveVoltDevice(blockValue) ?? throw new InvalidOperationException($"Block value {blockValue} does not provide {"IVoltDevice"}.");
		return voltElement.GetPL(voltDevice.GetStandardPL(blockValue), cellFace);
	}

	public float GetElementConsumerDemand(int blockValue, VoltElement voltElement, CellFace cellFace)
	{
		IVoltDevice voltDevice = VoltElement.ResolveVoltDevice(blockValue) ?? throw new InvalidOperationException($"Block value {blockValue} does not provide {"IVoltDevice"}.");
		return voltElement.GetConsumerDemand(voltDevice.GetStandardPL(blockValue), cellFace);
	}

	public float GetElementUL(int blockValue, VoltElement voltElement, CellFace cellFace)
	{
		IVoltDevice voltDevice = VoltElement.ResolveVoltDevice(blockValue) ?? throw new InvalidOperationException($"Block value {blockValue} does not provide {"IVoltDevice"}.");
		return voltElement.GetUL(voltDevice.GetStandardUL(blockValue), cellFace);
	}

	public float GetTransmissionCurrent()
	{
		if (m_inputVoltage <= 0f)
		{
			return 0f;
		}
		float num = 0f;
		bool flag = false;
		foreach (KeyValuePair<CellFace, VoltElement> containsElement in m_containsElements)
		{
			if (containsElement.Value != null && IsConsumer(containsElement.Value, containsElement.Key))
			{
				flag = true;
				num += m_outputToElements[containsElement.Value].P;
			}
		}
		if (!flag)
		{
			return 0f;
		}
		return num / m_inputVoltage;
	}

	public void OnRemove()
	{
		foreach (KeyValuePair<CellFace, VoltElement> containsElement in m_containsElements)
		{
			containsElement.Value.m_belongingConnections.Remove(this);
		}
	}
}
