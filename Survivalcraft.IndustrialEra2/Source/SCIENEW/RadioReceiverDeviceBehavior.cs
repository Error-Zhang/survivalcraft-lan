using Game;
using GameEntitySystem;

namespace SCIENEW;

public class RadioReceiverDeviceBehavior : DeviceBehavior
{
	public SubsystemElectricity m_subsystemElectricity;

	public SubsystemTerrain m_subsystemTerrain;

	public RadioReceiverDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemElectricity = project.FindSubsystem<SubsystemElectricity>(throwOnError: true);
		m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		int direction_Static = SixDirectionModelDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValue(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z));
		if (m_subsystemElectricity.GetElectricElement(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z, direction_Static) is RadioReceiverElectricElement receiver)
		{
			componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new RadioReceiverWidget(componentMiner.Inventory, receiver);
			AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
			return true;
		}
		return false;
	}
}
