using System.Runtime.CompilerServices;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW.Utils;

public static class BlockEntityUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Entity CreateBlockEntity(SubsystemTerrain subsystemTerrain, string entityName, Point3 position)
	{
		if (IndustrialEraLanAdapter.IsRemote(subsystemTerrain.Project))
		{
			return null;
		}
		DatabaseObject databaseObject = subsystemTerrain.Project.GameDatabase.Database.FindDatabaseObject(entityName, subsystemTerrain.Project.GameDatabase.EntityTemplateType, throwIfNotFound: true);
		ValuesDictionary valuesDictionary = new ValuesDictionary();
		valuesDictionary.PopulateFromDatabaseObject(databaseObject);
		valuesDictionary.GetValue<ValuesDictionary>("BlockEntity").SetValue("Coordinates", position);
		Entity entity = subsystemTerrain.Project.CreateEntity(valuesDictionary);
		subsystemTerrain.Project.AddEntity(entity);
		return entity;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RemoveBlockEntity(SubsystemTerrain subsystemTerrain, Point3 position)
	{
		if (IndustrialEraLanAdapter.IsRemote(subsystemTerrain.Project))
		{
			return;
		}
		ComponentBlockEntity blockEntity = subsystemTerrain.Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true).GetBlockEntity(position.X, position.Y, position.Z);
		if (blockEntity == null)
		{
			return;
		}
		Vector3 position2 = new Vector3(position) + new Vector3(0.5f);
		foreach (IInventory item in blockEntity.Entity.FindComponents<IInventory>())
		{
			item.DropAllItems(position2);
		}
		foreach (ITank item2 in blockEntity.Entity.FindComponents<ITank>())
		{
			item2.EmitAllFluids(position2);
		}
		subsystemTerrain.Project.RemoveEntity(blockEntity.Entity, disposeEntity: true);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool GetBlockEntity(SubsystemTerrain subsystemTerrain, Point3 position, out ComponentBlockEntity blockEntity)
	{
		blockEntity = subsystemTerrain.Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true).GetBlockEntity(position.X, position.Y, position.Z);
		return blockEntity != null;
	}
}
