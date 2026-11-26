using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class ToolsItemBehavior : ItemBehavior
{
	public SubsystemPickables m_subsystemPickables;

	public ToolsItemBehavior(Project project)
		: base(project)
	{
		m_subsystemPickables = project.FindSubsystem<SubsystemPickables>(throwOnError: true);
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		int activeBlockValue = componentMiner.ActiveBlockValue;
		TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Digging);
		if (ItemsBlockManager.GetItem(activeBlockValue) is WrenchsItem)
		{
			BodyRaycastResult? bodyRaycastResult = componentMiner.Raycast<BodyRaycastResult>(ray, RaycastMode.Interaction, raycastTerrain: false, raycastBodies: true, raycastMovingBlocks: false);
			if (!bodyRaycastResult.HasValue)
			{
				return false;
			}
			Matrix matrix = componentMiner.ComponentCreature.ComponentBody.Matrix;
			Vector3 position = matrix.Translation + 1f * matrix.Forward + Vector3.UnitY;
			if (!terrainRaycastResult.HasValue || bodyRaycastResult.Value.Distance < terrainRaycastResult.Value.Distance)
			{
				Entity entity = bodyRaycastResult.Value.ComponentBody.Entity;
				if (entity.FindComponent<ComponentTrain>() != null || entity.FindComponent<ComponentSteamBoat>() != null || entity.FindComponent<ComponentBoat>() != null)
				{
					string entityName = entity.ValuesDictionary.DatabaseObject.Name;
					if (!ItemsBlockManager.IdMap.TryGetValue(entityName == "Carriage" ? "Minecart" : entityName, out int itemValue)) return false;
					if (IndustrialEraLanAdapter.IsRemote(Project)) return true;
					Entity.FilteredComponentsEnumerator<IInventory> enumerator = entity.FindComponents<IInventory>().GetEnumerator();
					while (enumerator.MoveNext())
					{
						enumerator.Current.DropAllItems(position);
					}
					m_subsystemPickables.AddPickable(itemValue, 1, position, null, null);
					Project.RemoveEntity(entity, disposeEntity: true);
					return true;
				}
			}
		}
		return false;
	}
}
