using System.Collections.Generic;
using System.Threading.Tasks;
using Engine;
using Game;

namespace SCIENEW.VoltNet;

public abstract class SolarPanelVoltElement : VoltElement
{
	public SubsystemSolarPanelGroups m_subsystemBases;

	public SubsystemSolarPanelGroups.Cell m_thisCell;

	protected SolarPanelVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue)
		: base(subsystemVoltNet, subsystemTerrain, position, blockValue)
	{
		m_subsystemBases = subsystemVoltNet.Project.FindSubsystem<SubsystemSolarPanelGroups>(throwOnError: true);
		int value = Terrain.ReplaceLight(BlockValue, 0);
		m_thisCell = new SubsystemSolarPanelGroups.Cell(Position.X, Position.Y, Position.Z, value);
	}

	public override float GetUL(float standardUL, CellFace cellFace)
	{
		return standardUL;
	}

	public override float GetPL(float standardPL, CellFace cellFace)
	{
		return m_subsystemBases.GetBase(m_thisCell).GetCurrentPL(standardPL);
	}

	public override void OnAdded()
	{
		base.OnAdded();
		UpdateController();
	}

	public bool Connected()
	{
		return CalcConnection() > 0;
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		if (m_subsystemBases.IsThereABase(m_thisCell))
		{
			m_subsystemBases.RemoveBase(m_subsystemBases.GetBase(m_thisCell));
		}
		Point3 position = Position;
		for (int i = 0; i < 4; i++)
		{
			Point3 point = position + CellFace.FaceToPoint3(i);
			if (Terrain.ReplaceLight(SubsystemTerrain.Terrain.GetCellValueFast(point.X, point.Y, point.Z), 0) == m_thisCell.Value)
			{
				(SubsystemVoltNet.GetVoltElement(point) as SolarPanelVoltElement)?.UpdateController();
			}
		}
	}

	public async void UpdateController()
	{
		Dictionary<SubsystemSolarPanelGroups.Cell, SolarPanelVoltElement> all = new Dictionary<SubsystemSolarPanelGroups.Cell, SolarPanelVoltElement>();
		await GetConnectedSolarPanels(m_thisCell.X, m_thisCell.Y, m_thisCell.Z, m_thisCell.Value, all);
		foreach (SubsystemSolarPanelGroups.Cell key in all.Keys)
		{
			if (m_subsystemBases.IsThereABase(key))
			{
				m_subsystemBases.RemoveBase(m_subsystemBases.GetBase(m_thisCell));
			}
		}
		SubsystemSolarPanelGroups subsystemBases = m_subsystemBases;
		HashSet<SubsystemSolarPanelGroups.Cell> hashSet = new HashSet<SubsystemSolarPanelGroups.Cell>();
		foreach (SubsystemSolarPanelGroups.Cell key2 in all.Keys)
		{
			hashSet.Add(key2);
		}
		subsystemBases.AddBaseByPoints(hashSet, new SubsystemSolarPanelGroups.Base(all, SubsystemTerrain.Project));
	}

	public async Task GetConnectedSolarPanels(int x, int y, int z, int panelValue, Dictionary<SubsystemSolarPanelGroups.Cell, SolarPanelVoltElement> parent)
	{
		if (parent.ContainsKey(new SubsystemSolarPanelGroups.Cell(x, y, z, panelValue)))
		{
			return;
		}
		SolarPanelVoltElement value = SubsystemVoltNet.GetVoltElement(new Point3(x, y, z)) as SolarPanelVoltElement;
		parent.Add(new SubsystemSolarPanelGroups.Cell(x, y, z, panelValue), value);
		Point3 center = new Point3(x, y, z);
		for (int i = 0; i < 4; i++)
		{
			Point3 point = center + CellFace.FaceToPoint3(i);
			if (Terrain.ReplaceLight(SubsystemTerrain.Terrain.GetCellValueFast(point.X, point.Y, point.Z), 0) == panelValue)
			{
				await GetConnectedSolarPanels(point.X, point.Y, point.Z, panelValue, parent);
			}
		}
	}
}
