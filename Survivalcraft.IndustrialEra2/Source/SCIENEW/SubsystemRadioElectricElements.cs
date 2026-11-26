using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemRadioElectricElements : Subsystem
{
	public const uint MinFrequency = 1000u;

	public const uint MaxFrequency = 50000u;

	public SubsystemElectricity m_subsystemElectricity;

	public readonly Dictionary<uint, List<RadioTransmitterElectricElement>> m_transmitter = new Dictionary<uint, List<RadioTransmitterElectricElement>>();

	public readonly Dictionary<uint, List<RadioReceiverElectricElement>> m_receiver = new Dictionary<uint, List<RadioReceiverElectricElement>>();

	public readonly Dictionary<Point3, uint> m_frequenciesInFile = new Dictionary<Point3, uint>();

	public readonly Dictionary<Point3, bool> m_enablesInFile = new Dictionary<Point3, bool>();

	public Dictionary<Point3, uint> FrequenciesInFile => m_frequenciesInFile;

	public Dictionary<Point3, bool> EnablesInFile => m_enablesInFile;

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemElectricity = Project.FindSubsystem<SubsystemElectricity>(throwOnError: true);
		int value = valuesDictionary.GetValue("ElementsCount", 0);
		if (value > 0)
		{
			ValuesDictionary value2 = valuesDictionary.GetValue<ValuesDictionary>("Positions");
			ValuesDictionary value3 = valuesDictionary.GetValue<ValuesDictionary>("Frequencies");
			ValuesDictionary value4 = valuesDictionary.GetValue<ValuesDictionary>("Enables");
			for (int i = 0; i < value; i++)
			{
				Point3 value5 = value2.GetValue<Point3>(i.ToString(CultureInfo.InvariantCulture));
				uint value6 = value3.GetValue<uint>(i.ToString(CultureInfo.InvariantCulture));
				bool value7 = value4.GetValue<bool>(i.ToString(CultureInfo.InvariantCulture));
				m_frequenciesInFile.Add(value5, value6);
				m_enablesInFile.Add(value5, value7);
			}
		}
	}

	public override void Save(ValuesDictionary valuesDictionary)
	{
		base.Save(valuesDictionary);
		int num = 0;
		ValuesDictionary valuesDictionary2 = new ValuesDictionary();
		ValuesDictionary valuesDictionary3 = new ValuesDictionary();
		ValuesDictionary valuesDictionary4 = new ValuesDictionary();
		foreach (RadioTransmitterElectricElement item in m_transmitter.SelectMany((KeyValuePair<uint, List<RadioTransmitterElectricElement>> transmitters) => transmitters.Value))
		{
			valuesDictionary2.SetValue(num.ToString(CultureInfo.InvariantCulture), item.m_cellFace.Point);
			valuesDictionary3.SetValue(num.ToString(CultureInfo.InvariantCulture), item.Frequency);
			valuesDictionary4.SetValue(num.ToString(CultureInfo.InvariantCulture), item.Enable);
			num++;
		}
		foreach (RadioReceiverElectricElement item2 in m_receiver.SelectMany((KeyValuePair<uint, List<RadioReceiverElectricElement>> receivers) => receivers.Value))
		{
			valuesDictionary2.SetValue(num.ToString(CultureInfo.InvariantCulture), item2.m_cellFace.Point);
			valuesDictionary3.SetValue(num.ToString(CultureInfo.InvariantCulture), item2.Frequency);
			valuesDictionary4.SetValue(num.ToString(CultureInfo.InvariantCulture), item2.Enable);
			num++;
		}
		valuesDictionary.SetValue("Positions", valuesDictionary2);
		valuesDictionary.SetValue("Frequencies", valuesDictionary3);
		valuesDictionary.SetValue("Enables", valuesDictionary4);
		valuesDictionary.SetValue("ElementsCount", num);
	}

	public void Add(uint frequency, RadioTransmitterElectricElement electricElement)
	{
		m_transmitter.TryGetValue(frequency, out List<RadioTransmitterElectricElement> value);
		if (value == null)
		{
			value = new List<RadioTransmitterElectricElement>();
			m_transmitter.Add(frequency, value);
		}
		m_transmitter[frequency].Add(electricElement);
	}

	public void Add(uint frequency, RadioReceiverElectricElement electricElement)
	{
		m_receiver.TryGetValue(frequency, out List<RadioReceiverElectricElement> value);
		if (value == null)
		{
			value = new List<RadioReceiverElectricElement>();
			m_receiver.Add(frequency, value);
		}
		m_receiver[frequency].Add(electricElement);
	}

	public void Remove(uint frequency, RadioTransmitterElectricElement electricElement)
	{
		m_transmitter[frequency].Remove(electricElement);
	}

	public void Remove(uint frequency, RadioReceiverElectricElement electricElement)
	{
		m_receiver[frequency].Remove(electricElement);
	}

	public void ChangeFrequency(RadioTransmitterElectricElement electricElement, uint oldF, uint newF)
	{
		Remove(oldF, electricElement);
		Add(newF, electricElement);
	}

	public void ChangeFrequency(RadioReceiverElectricElement electricElement, uint oldF, uint newF)
	{
		Remove(oldF, electricElement);
		Add(newF, electricElement);
	}

	public void QueueReceiversSimulate(uint frequency)
	{
		if (m_receiver.TryGetValue(frequency, out List<RadioReceiverElectricElement> value))
		{
			value?.ForEach((RadioReceiverElectricElement x) =>
			{
				m_subsystemElectricity.QueueElectricElementForSimulation(x, m_subsystemElectricity.CircuitStep + 1);
			});
		}
	}

	public bool GetTransmitter(uint frequency, out List<RadioTransmitterElectricElement> transmitters)
	{
		return m_transmitter.TryGetValue(frequency, out transmitters);
	}
}
