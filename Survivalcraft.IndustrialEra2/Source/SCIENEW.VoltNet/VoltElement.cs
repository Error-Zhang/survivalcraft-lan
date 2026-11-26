using System;
using System.Collections.Generic;
using Engine;
using Game;
using RecipaediaEX.Events;
using SCIENEW.Events;

namespace SCIENEW.VoltNet;

public abstract class VoltElement
{
	public SubsystemVoltNet SubsystemVoltNet;

	public SubsystemTerrain SubsystemTerrain;

	public Point3 Position;

	public int BlockValue;

	protected float m_standardVoltage;

	protected float m_standardPower;

	public HashSet<VoltConnection> m_belongingConnections = new HashSet<VoltConnection>();

	public WorkState CurrentWorkState
	{
		get;
		protected set
		{
			if (field != value)
			{
				WorkState previous = field;
				field = value;
				if (SubsystemVoltNet?.Project != null)
				{
					RecipaediaEventBus.GetPublisher<VoltElementWorkStateChangedEvent>().Publish(new VoltElementWorkStateChangedEvent(SubsystemVoltNet.Project, Position, BlockValue, previous, value));
				}
			}
		}
	} = WorkState.Underpowered;

	public VoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
	{
		SubsystemTerrain = subsystemTerrain;
		SubsystemVoltNet = subsystemVoltNet;
		Position = position;
		BlockValue = blockValue;
		ResetWorkState();
		IVoltDevice voltDevice = ResolveVoltDevice(BlockValue) ?? throw new InvalidOperationException($"Block value {BlockValue} does not provide {"IVoltDevice"}.");
		m_standardVoltage = voltDevice.GetStandardUL(BlockValue);
		m_standardPower = voltDevice.GetStandardPL(BlockValue);
	}

	internal static IVoltDevice? ResolveVoltDevice(int blockValue)
	{
		return BlockInterfaceResolver.ResolveFromValue<IVoltDevice>(blockValue);
	}

	public virtual void Simulate(float dt)
	{
		CalcInputVoltageAndPower(out var voltage, out var power, out var voltageMatch);
		if (!voltageMatch)
		{
			HandleVoltageMismatch();
			return;
		}
		UpdateWorkState(power, voltage, m_standardPower, m_standardVoltage);
		Update(dt, voltage, power);
	}

	public virtual void Update(float dt, float inputVoltage, float inputPower)
	{
	}

	public virtual float GetPL(float standardPL, CellFace cellFace)
	{
		return standardPL;
	}

	public virtual float GetConsumerDemand(float standardPL, CellFace cellFace)
	{
		float pL = GetPL(standardPL, cellFace);
		if (!(pL < 0f))
		{
			return 0f;
		}
		return MathF.Abs(pL);
	}

	public virtual float GetUL(float standardUL, CellFace cellFace)
	{
		return standardUL;
	}

	public virtual void OnAdded()
	{
	}

	public virtual void OnRemoved()
	{
	}

	public virtual void UpdateWorkState(float inputPower, float inputVoltage, float requiredPower, float requiredVoltage, float tolerance = 0.05f)
	{
		float num = requiredVoltage * tolerance;
		float num2 = requiredPower * tolerance;
		if (inputVoltage < requiredVoltage - num)
		{
			CurrentWorkState = WorkState.Underpowered;
		}
		else if (inputVoltage > requiredVoltage + num)
		{
			CurrentWorkState = WorkState.Overloaded;
		}
		else if (inputPower < requiredPower - num2)
		{
			CurrentWorkState = WorkState.Underpowered;
		}
		else
		{
			CurrentWorkState = WorkState.Active;
		}
	}

	private (float Voltage, float Power, bool Match)? m_lanMeasurements;

	internal void ApplyLanState(WorkState workState, float voltage, float power, bool match)
	{
		m_lanMeasurements = (voltage, power, match);
		CurrentWorkState = workState;
	}

	public void CalcInputVoltageAndPower(out float voltage, out float power, out bool voltageMatch)
	{
		if (m_lanMeasurements is { } measurements && IndustrialEraLanAdapter.IsRemote(SubsystemVoltNet.Project))
		{
			(voltage, power, voltageMatch) = measurements;
			return;
		}
		voltage = 0f;
		power = 0f;
		voltageMatch = true;
		foreach (VoltConnection belongingConnection in m_belongingConnections)
		{
			if (belongingConnection.m_outputToElements.TryGetValue(this, out (float, float) value))
			{
				if (voltage == 0f)
				{
					(voltage, _) = value;
				}
				if (voltage != value.Item1 && value.Item1 != 0f)
				{
					voltageMatch = false;
					break;
				}
				power += value.Item2;
			}
		}
	}

	public int CalcConnection()
	{
		return m_belongingConnections.Count;
	}

	public int CalcConsumerConnectionCount()
	{
		int num = 0;
		foreach (VoltConnection belongingConnection in m_belongingConnections)
		{
			if (belongingConnection.HasConsumerDemand())
			{
				num++;
			}
		}
		return num;
	}

	public virtual void ResetWorkState()
	{
		CurrentWorkState = WorkState.Underpowered;
	}

	internal void ApplyConnectionVoltageFault()
	{
		CurrentWorkState = WorkState.Broken;
	}

	public virtual void HandleVoltageMismatch()
	{
		CurrentWorkState = WorkState.Broken;
	}
}
