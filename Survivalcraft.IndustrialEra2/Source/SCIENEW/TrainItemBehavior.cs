using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class TrainItemBehavior : ItemBehavior
{
	public TrainItemBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		int activeBlockValue = componentMiner.ActiveBlockValue;
		bool isCarriage = activeBlockValue == ItemsBlockManager.IdMap["Minecart"];
		if (!isCarriage && activeBlockValue != ItemsBlockManager.IdMap["Train"]) return false;

		TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Digging, raycastBodies: false);
		if (terrainRaycastResult.HasValue && Terrain.ExtractContents(terrainRaycastResult.Value.Value) == RailBlock.Index)
		{
			if (IndustrialEraLanAdapter.IsRemote(Project)) return true;
			Vector3 position = new Vector3(terrainRaycastResult.Value.CellFace.Point) + new Vector3(0.5f);
			string entityTemplateName = isCarriage ? "Carriage" : "Train";
			Entity entity = DatabaseManager.CreateEntity(Project, entityTemplateName, throwIfNotFound: true);
			Vector3 forwardVector = componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation.GetForwardVector();
			entity.FindComponent<ComponentTrain>(throwOnError: true).SetDirection((!RailBlock.IsDirectionX(RailBlock.GetRailType(Terrain.ExtractData(terrainRaycastResult.Value.Value)))) ? ((forwardVector.X < 0f) ? 1 : 3) : ((!(forwardVector.Z < 0f)) ? 2 : 0));
			entity.FindComponent<ComponentBody>(throwOnError: true).Position = position;
			entity.FindComponent<ComponentSpawn>(throwOnError: true).SpawnDuration = 0f;
			Project.AddEntity(entity);
			ComponentTrain componentTrain = entity.FindComponent<ComponentTrain>(throwOnError: true);
			if (isCarriage)
			{
				componentTrain.Update(0f);
				ComponentTrain componentTrain2 = componentTrain.FindNearestTrain();
				if (componentTrain2 != null)
				{
					componentTrain.ParentBody = componentTrain2;
				}
			}
			componentMiner.RemoveActiveTool(1);
			return true;
		}
		return false;
	}
}
