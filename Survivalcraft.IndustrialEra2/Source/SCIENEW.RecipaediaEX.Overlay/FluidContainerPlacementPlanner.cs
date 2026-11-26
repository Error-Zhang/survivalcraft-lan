using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX.Overlay;

namespace SCIENEW.RecipaediaEX.Overlay;

internal static class FluidContainerPlacementPlanner {
    private const float VolumeEpsilon = 0.05f;

    public static PlacementResult TryPlaceEquationFluids(ITank tank, int tankStart, int tankCount,
        EquationFluid[] materials, PlacementSources sources, bool execute) {
        if (materials.Length == 0) {
            return PlacementResult.AlreadySatisfied();
        }
        IInventory inventory = sources.PlayerInventory;
        if (inventory == null) {
            return PlacementResult.None(["缺少玩家背包"]);
        }
        (int Value, float Volume)[] tanks = new (int, float)[tankCount];
        for (int i = 0; i < tankCount; i++) {
            tanks[i] = (tank.GetTankValue(tankStart + i), tank.GetTankVolume(tankStart + i));
        }
        int[] containers = new int[inventory.SlotsCount];
        for (int i = 0; i < containers.Length; i++) {
            containers[i] = inventory.GetSlotCount(i);
        }

        List<string> missing = new();
        bool moved = false;
        foreach (EquationFluid material in materials) {
            int index = FindTank(tank, tankStart, tanks, material.Value);
            if (index >= 0 && tank.GetTankCapacity(tankStart + index, material.Value) > VolumeEpsilon) {
                float volume = tanks[index].Volume;
                float required = material.Volume - volume;
                if (required <= VolumeEpsilon) {
                    continue;
                }
                float filled = Fill(inventory, containers, tank, tankStart + index, material.Value, volume, required, execute);
                if (filled > VolumeEpsilon) {
                    tanks[index] = (material.Value, volume + filled);
                    moved = true;
                }
                if (filled + VolumeEpsilon >= required) {
                    continue;
                }
            }
            string label = FluidsManager.GetFluid(material.Value)?.GetDisplayName(material.Value) ?? $"流体 {material.Value}";
            string message = "缺少 " + label;
            if (!missing.Contains(message)) {
                missing.Add(message);
            }
        }
        return missing.Count == 0 ? PlacementResult.Complete(moved)
            : moved ? PlacementResult.Partial(missing) : PlacementResult.None(missing);
    }

    private static int FindTank(ITank tank, int start, (int Value, float Volume)[] tanks, int wanted) {
        int empty = -1;
        for (int i = 0; i < tanks.Length; i++) {
            if (tanks[i].Volume > VolumeEpsilon) {
                if (tanks[i].Value == wanted) {
                    return i;
                }
            }
            else if (empty < 0 && tank.GetTankCapacity(start + i, wanted) > VolumeEpsilon) {
                empty = i;
            }
        }
        return empty;
    }

    private static float Fill(IInventory inventory, int[] containers, ITank tank, int tankIndex,
        int fluid, float initialVolume, float required, bool execute) {
        if (FluidsManager.GetFluid(fluid) is not IScoopableFluid scoopable) {
            return 0;
        }
        BiDictionary<int, int> pairs = scoopable.AcquireContainer(fluid);
        float capacity = tank.GetTankCapacity(tankIndex, fluid);
        float filled = 0;
        for (int slot = 0; slot < containers.Length && filled + VolumeEpsilon < required; slot++) {
            int fullContainer = inventory.GetSlotValue(slot);
            if (containers[slot] == 0 || !pairs.TryGetBySecond(fullContainer, out int emptyContainer)) {
                continue;
            }
            float unit = FluidsManager.GetContainerVolume(fullContainer);
            if (unit <= VolumeEpsilon) {
                continue;
            }
            int remaining = (int)MathF.Ceiling((required - filled - VolumeEpsilon) / unit);
            int fits = (int)MathF.Floor((capacity - initialVolume - filled + VolumeEpsilon) / unit);
            int count = Math.Min(containers[slot], Math.Min(remaining, fits));
            if (count <= 0) {
                continue;
            }
            if (execute) {
                int removed = inventory.RemoveSlotItems(slot, count);
                if (removed == 0) {
                    continue;
                }
                float before = tank.GetTankVolume(tankIndex);
                tank.AddTankFluid(tankIndex, fluid, unit * removed, out _);
                float added = Math.Max(0, tank.GetTankVolume(tankIndex) - before);
                count = Math.Min(removed, (int)MathF.Floor((added + VolumeEpsilon) / unit));
                if (added > count * unit + VolumeEpsilon) {
                    tank.RemoveTankFluid(tankIndex, added - count * unit);
                }
                if (removed > count) {
                    inventory.AddSlotItems(slot, fullContainer, removed - count);
                }
                int overflow = ComponentInventoryBase.AcquireItems(inventory, emptyContainer, count);
                if (overflow > 0 && inventory is Component component
                    && component.Entity.FindComponent<ComponentPlayer>() is ComponentPlayer player) {
                    player.Project.FindSubsystem<SubsystemPickables>().AddPickable(emptyContainer, overflow, player.ComponentBody.Position, Vector3.Zero, null);
                }
            }
            containers[slot] -= count;
            filled += unit * count;
        }
        return filled;
    }
}
