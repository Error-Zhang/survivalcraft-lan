using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCondenser : Component, ICondenserComponent
{
	public enum Mode
	{
		Charge,
		Discharge
	}

	public bool ElectricalControl;

	private readonly HashSet<int> m_portFaces = new HashSet<int>();

	private ComponentBlockEntity m_componentBlockEntity;

	private SubsystemTerrain m_subsystemTerrain;

	public float StoredEnergy { get; set; }

	public float OutputPower => 3f;

	public float OutputVoltage => 1f;

	public bool HasEnergy => StoredEnergy > 0f;

	public Mode CurrentMode { get; set; }

	public virtual float Capacitance => 1000f;

	public void InputEnergy(float energy)
	{
		StoredEnergy = Math.Clamp(StoredEnergy + energy, 0f, Capacitance);
	}

	public float OutputEnergy(float energy)
	{
		float num = MathUtils.Min(StoredEnergy, energy);
		StoredEnergy -= num;
		return num;
	}

	public void RebindPorts()
	{
		m_portFaces.Clear();
		if (m_componentBlockEntity == null || m_subsystemTerrain == null)
		{
			return;
		}
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		for (int i = 0; i < 6; i++)
		{
			Point3 point = coordinates + CellFace.FaceToPoint3(i);
			int cellValue = m_subsystemTerrain.Terrain.GetCellValue(point.X, point.Y, point.Z);
			if (CondenserPortUtils.GetPortRole(cellValue).HasValue)
			{
				int innerData = DevicesUtils.GetInnerData(cellValue);
				if (!(point + CellFace.FaceToPoint3(CellFace.OppositeFace(innerData)) != coordinates) && innerData == i)
				{
					m_portFaces.Add(i);
				}
			}
		}
	}

	public bool HasPortOnFace(int hostFace)
	{
		return m_portFaces.Contains(hostFace);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		StoredEnergy = valuesDictionary.GetValue("StoredEnergy", 0f);
		CurrentMode = valuesDictionary.GetValue("Mode", Mode.Charge);
		ElectricalControl = valuesDictionary.GetValue("ElectricalControl", defaultValue: false);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		RebindPorts();
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("StoredEnergy", StoredEnergy);
		valuesDictionary.SetValue("Mode", CurrentMode);
		valuesDictionary.SetValue("ElectricalControl", ElectricalControl);
	}
}
