using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class RadarDeviceBehavior : EntityDeviceBehavior<ComponentRadar>
{
	public RadarDeviceBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		if (componentMiner.Raycast(ray, RaycastMode.Digging) is BodyRaycastResult bodyRaycastResult)
		{
			Entity entity = bodyRaycastResult.ComponentBody.Entity;
			ComponentMount componentMount = entity.FindComponent<ComponentMount>();
			if (componentMount != null)
			{
				if (IndustrialEraLanAdapter.IsRemote(Project)) return true;
				entity = DatabaseManager.CreateEntity(Project, "PortableRadar", throwIfNotFound: true);
				entity.FindComponent<ComponentSpawn>(throwOnError: true).SpawnDuration = 0f;
				entity.FindComponent<ComponentFrame>(throwOnError: true).Position = componentMount.MountOffset + componentMount.Entity.FindComponent<ComponentBody>(throwOnError: true).Position;
				entity.FindComponent<ComponentDeviceRider>(throwOnError: true).StartMounting(componentMount);
				Project.AddEntity(entity);
				return true;
			}
		}
		return false;
	}
}
