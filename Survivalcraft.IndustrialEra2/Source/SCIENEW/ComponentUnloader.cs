using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentUnloader : ComponentInventoryBase
{
	protected ComponentBlockEntity m_componentBlockEntity;

	public SubsystemPlayers SubsystemPlayers;

	public SubsystemTerrain SubsystemTerrain;

	public SubsystemPickables SubsystemPickables;

	public bool DispenseItem = true;

	public bool Place()
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		int cellValue = SubsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z);
		int num = 0;
		int slotValue;
		while (true)
		{
			if (num >= SlotsCount)
			{
				return false;
			}
			slotValue = GetSlotValue(num);
			int slotCount = GetSlotCount(num);
			if (slotValue != 0 && slotCount > 0)
			{
				break;
			}
			num++;
		}
		int innerData = DevicesUtils.GetInnerData(cellValue);
		num = RemoveSlotItems(num, 1);
		while (num-- > 0)
		{
			Vector3 vector = new Vector3(coordinates) + new Vector3(0.5f);
			Vector3 vector2 = CellFace.FaceToVector3(innerData);
			if (!Place(vector + vector2, innerData, slotValue) && DispenseItem)
			{
				Vector3 position = vector + 0.6f * vector2;
				SubsystemPickables.AddPickable(slotValue, 1, position, 1.8f * (vector2 + m_random.Vector3(0.2f)), null);
			}
		}
		return true;
	}

	public bool Place(Vector3 position, int face, int value)
	{
		TerrainRaycastResult? terrainRaycastResult = SubsystemTerrain.Raycast(position, position + CellFace.FaceToVector3(face) * 8f, useInteractionBoxes: true, skipAirBlocks: true, null);
		ComponentMiner componentMiner = SubsystemPlayers.FindNearestPlayer(position).ComponentMiner;
		float pokingPhase = componentMiner.PokingPhase;
		if (terrainRaycastResult.HasValue && componentMiner.Place(terrainRaycastResult.Value, value))
		{
			componentMiner.PokingPhase = pokingPhase;
			if (componentMiner.ComponentCreature.PlayerStats != null)
			{
				componentMiner.ComponentCreature.PlayerStats.BlocksPlaced--;
			}
			return true;
		}
		return false;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		SubsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(throwOnError: true);
		SubsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		SubsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
	}
}
