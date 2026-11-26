using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.VoltNet;

namespace SCIENEW;

public class GalvanicCellDeviceBehavior : DeviceBehavior
{
	public SubsystemVoltNet m_subsystemVoltNet;

	public GalvanicCellDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemVoltNet = project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		if (m_subsystemVoltNet.GetVoltElement(raycastResult.CellFace.Point) is GalvanicCellVoltElement { m_energy: var energy })
		{
			componentMiner.ComponentPlayer.ComponentGui.DisplaySmallMessage(string.Format(LanguageControl.Get("Behaviors", "GalvanicCellDeviceBehavior", "RemainingEnergyLevel"), energy), Color.White, blinking: false, playNotificationSound: true);
			return true;
		}
		return false;
	}
}
