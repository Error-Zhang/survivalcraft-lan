using Engine;
using Game;

namespace SCIENEW.VoltNet;

public class RadioVoltElement : VoltElement
{
	private SubsystemElectricity m_subsystemElectricity;

	private CellFace m_mountingCellFace;

	public bool m_isActive;

	public RadioVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		int direction_Static = SixDirectionModelDevice.GetDirection_Static(blockValue);
		m_mountingCellFace = new CellFace(position.X, position.Y, position.Z, direction_Static);
		m_subsystemElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemElectricity>(throwOnError: true);
	}

	internal ElectricElement GetElectricElement() => m_subsystemElectricity.GetElectricElement(
		m_mountingCellFace.X, m_mountingCellFace.Y, m_mountingCellFace.Z, m_mountingCellFace.Face);

	internal int GetFrequency() => GetElectricElement() switch
	{
		RadioReceiverElectricElement receiver => (int)receiver.Frequency,
		RadioTransmitterElectricElement transmitter => (int)transmitter.Frequency,
		_ => 0
	};

	internal static void SetFrequency(ElectricElement element, int frequency)
	{
		switch (element)
		{
			case RadioReceiverElectricElement receiver when receiver.Frequency != frequency:
				receiver.Frequency = (uint)frequency;
				break;
			case RadioTransmitterElectricElement transmitter when transmitter.Frequency != frequency:
				transmitter.Frequency = (uint)frequency;
				break;
		}
	}

	internal void SetRadioActive(bool active)
	{
		m_isActive = active;
		switch (GetElectricElement())
		{
			case RadioReceiverElectricElement receiver:
				receiver.Enable = active;
				break;
			case RadioTransmitterElectricElement transmitter:
				transmitter.Enable = active;
				break;
		}
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		return 0f - standardPL;
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		return 0f - standardUL;
	}

	public override void Update(float dt, float inputVoltage, float inputPower)
	{
		base.Update(dt, inputVoltage, inputPower);
		bool flag = CurrentWorkState == WorkState.Active;
		if (flag != m_isActive)
		{
			SetRadioActive(flag);
		}
	}
}
