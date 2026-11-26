using GameEntitySystem;

namespace SCIENEW;

public class SolarPanelDeviceBehavior : DeviceBehavior
{
	public SubsystemSolarPanelGroups m_subsystemSolarPanelGroups;

	public SolarPanelDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemSolarPanelGroups = project.FindSubsystem<SubsystemSolarPanelGroups>(throwOnError: true);
	}
}
