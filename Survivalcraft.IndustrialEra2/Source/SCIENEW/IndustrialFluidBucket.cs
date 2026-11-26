using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

internal static class IndustrialFluidBucket
{
    internal static bool Fill(SubsystemTerrain terrain, ComponentMiner miner, CellFace target, int filledBucket)
    {
        IInventory inventory = miner.Inventory;
        int slot = inventory.ActiveSlotIndex;
        int count = inventory.GetSlotCount(slot);
        int destination = count == 1 || inventory is ComponentCreativeInventory
            ? slot : ComponentInventoryBase.FindAcquireSlotForItem(inventory, filledBucket);
        if (count <= 0 || destination < 0 || inventory.GetSlotCapacity(destination, filledBucket) == 0)
        {
            return false;
        }
        if (IndustrialEraLanAdapter.IsRemote(terrain.Project)) return true;
        if (inventory.RemoveSlotItems(slot, 1) != 1) return false;

        inventory.AddSlotItems(destination, filledBucket, 1);
        terrain.DestroyCell(0, target.X, target.Y, target.Z, 0, noDrop: false, noParticleSystem: false);
        return true;
    }

    internal static bool Pour(Project project, ComponentMiner miner, Ray3 ray, int fluid)
    {
        IInventory inventory = miner.Inventory;
        TerrainRaycastResult? target = miner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Interaction);
        if (target == null || inventory.GetSlotCount(inventory.ActiveSlotIndex) == 0) return false;
        if (IndustrialEraLanAdapter.IsRemote(project)) return true;
        if (!miner.Place(target.Value, Terrain.MakeBlockValue(fluid))) return false;

        inventory.RemoveSlotItems(inventory.ActiveSlotIndex, 1);
        int left = ComponentInventoryBase.AcquireItems(inventory, Terrain.MakeBlockValue(EmptyBucketBlock.Index), 1);
        if (left > 0)
        {
            project.FindSubsystem<SubsystemPickables>(throwOnError: true).AddPickable(
                Terrain.MakeBlockValue(EmptyBucketBlock.Index), left, miner.ComponentCreature.ComponentBody.Position, null, null);
        }
        return true;
    }
}
