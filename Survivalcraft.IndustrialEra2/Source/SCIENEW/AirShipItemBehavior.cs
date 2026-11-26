using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class AirShipItemBehavior : ItemBehavior
{
	public AirShipItemBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		int activeBlockValue = componentMiner.ActiveBlockValue;
		TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Digging);
		if (terrainRaycastResult.HasValue)
		{
			Vector3 position = terrainRaycastResult.Value.HitPoint();
			if (ItemsBlockManager.GetItem(activeBlockValue) is AirshipItem)
			{
			if (IndustrialEraLanAdapter.IsRemote(Project)) return true;
				Entity entity = DatabaseManager.CreateEntity(Project, "Airship", throwIfNotFound: true);
				entity.FindComponent<ComponentFrame>(throwOnError: true).Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0f);
				entity.FindComponent<ComponentBody>(throwOnError: true).Position = position;
				entity.FindComponent<ComponentSpawn>(throwOnError: true).SpawnDuration = 0f;
				Project.AddEntity(entity);
				componentMiner.RemoveActiveTool(1);
				return true;
			}
		}
		return false;
	}
}
