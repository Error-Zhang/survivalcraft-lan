using System;
using System.Collections.Generic;
using Game;
using RecipaediaEX.Overlay;
using SCIENEW.Utils;

namespace SCIENEW.RecipaediaEX.Overlay;

internal static class ContainerSlotPlacementPlanner
{
	private sealed class SlotAction
	{
		public int TargetSlot;

		public int SourceSlot = -1;

		public int BlockValue;

		public int Count;

		public bool NeedsTransfer;

		public string? MissingLabel;
	}

	public static PlacementResult TryPlaceSingleIngredientSlot(IInventory container, int targetSlot, string ingredient, int requiredCount, PlacementSources sources, PlacementOptions options, bool execute)
	{
		return TryPlaceIngredientSlot(container, targetSlot, ingredient, requiredCount, sources, options, execute, options.ClearGridBeforePlace);
	}

	public static PlacementResult TryPlaceEquationItems(IInventory container, int slotStart, int slotCount, EquationItem[] materials, PlacementSources sources, PlacementOptions options, bool execute)
	{
		if (sources.PlayerInventory == null)
		{
			return PlacementResult.None(new _003C_003Ez__ReadOnlySingleElementList<string>("缺少玩家背包"));
		}
		if (materials.Length == 0)
		{
			return PlacementResult.Complete(hadTransfers: false);
		}
		List<SlotAction> list = new List<SlotAction>();
		bool[] array = new bool[slotCount];
		for (int i = 0; i < materials.Length; i++)
		{
			EquationItem equationItem = materials[i];
			int[] array2 = equationItem.Values ?? Array.Empty<int>();
			if (array2.Length == 0)
			{
				list.Add(new SlotAction
				{
					MissingLabel = "缺少原料"
				});
				continue;
			}
			int num = FindTargetSlot(container, slotStart, slotCount, array2, equationItem.Count, options, array);
			if (num < 0)
			{
				list.Add(new SlotAction
				{
					MissingLabel = FormatMissingValues(array2, equationItem.Count)
				});
			}
			else
			{
				int num2 = num - slotStart;
				array[num2] = true;
				BuildSlotPlanForValues(container, num, array2, equationItem.Count, sources, options, list, treatAsEmpty: false);
			}
		}
		return ExecutePlan(container, sources, list, execute);
	}

	private static PlacementResult TryPlaceIngredientSlot(IInventory container, int targetSlot, string ingredient, int requiredCount, PlacementSources sources, PlacementOptions options, bool execute, bool replaceMismatched)
	{
		if (sources.PlayerInventory == null)
		{
			return PlacementResult.None(new _003C_003Ez__ReadOnlySingleElementList<string>("缺少玩家背包"));
		}
		if ((replaceMismatched & execute) && !TryReturnMismatchedSlotContents(container, targetSlot, ingredient, requiredCount, sources.PlayerInventory, out string error))
		{
			return PlacementResult.None(new _003C_003Ez__ReadOnlySingleElementList<string>(error ?? "背包已满，无法取出机器内原料"));
		}
		bool treatAsEmpty = replaceMismatched && !execute;
		List<SlotAction> plan = new List<SlotAction>();
		BuildSlotPlanForIngredient(container, targetSlot, ingredient, requiredCount, sources, options, plan, treatAsEmpty);
		return ExecutePlan(container, sources, plan, execute);
	}

	private static int FindTargetSlot(IInventory container, int slotStart, int slotCount, int[] accepted, int requiredCount, PlacementOptions options, bool[] claimed)
	{
		for (int i = 0; i < slotCount; i++)
		{
			if (claimed[i])
			{
				continue;
			}
			int num = slotStart + i;
			int slotValue = container.GetSlotValue(num);
			int slotCount2 = container.GetSlotCount(num);
			if (slotValue != 0 && !MatchesAny(slotValue, accepted))
			{
				if (!options.FillEmptyOnly)
				{
				}
			}
			else if ((slotValue == 0 || slotCount2 < requiredCount) && (slotValue == 0 || MatchesAny(slotValue, accepted)))
			{
				return num;
			}
		}
		for (int j = 0; j < slotCount; j++)
		{
			if (!claimed[j])
			{
				int num2 = slotStart + j;
				if (container.GetSlotCount(num2) == 0)
				{
					return num2;
				}
			}
		}
		return -1;
	}

	private static void BuildSlotPlanForIngredient(IInventory container, int targetSlot, string ingredient, int requiredCount, PlacementSources sources, PlacementOptions options, List<SlotAction> plan, bool treatAsEmpty)
	{
		int num = ((!treatAsEmpty) ? container.GetSlotValue(targetSlot) : 0);
		int num2 = ((!treatAsEmpty) ? container.GetSlotCount(targetSlot) : 0);
		int num3 = requiredCount;
		if (num != 0 && MatchesIngredient(ingredient, num))
		{
			int slotCapacity = container.GetSlotCapacity(targetSlot, num);
			num3 = ((num2 < slotCapacity) ? ((num2 >= requiredCount) ? 1 : (requiredCount - num2)) : 0);
		}
		else if (num != 0 && options.FillEmptyOnly)
		{
			plan.Add(new SlotAction
			{
				MissingLabel = FormatMissingIngredient(ingredient, requiredCount)
			});
			return;
		}
		if (num3 <= 0)
		{
			plan.Add(new SlotAction
			{
				TargetSlot = targetSlot
			});
			return;
		}
		if (!TryFindPlayerStackForIngredient(sources.PlayerInventory, ingredient, num3, out var sourceSlot, out var blockValue, out var transferCount))
		{
			plan.Add(new SlotAction
			{
				MissingLabel = FormatMissingIngredient(ingredient, num3)
			});
			return;
		}
		plan.Add(new SlotAction
		{
			TargetSlot = targetSlot,
			SourceSlot = sourceSlot,
			BlockValue = blockValue,
			Count = transferCount,
			NeedsTransfer = true
		});
	}

	private static void BuildSlotPlanForValues(IInventory container, int targetSlot, int[] accepted, int requiredCount, PlacementSources sources, PlacementOptions options, List<SlotAction> plan, bool treatAsEmpty)
	{
		int num = ((!treatAsEmpty) ? container.GetSlotValue(targetSlot) : 0);
		int num2 = ((!treatAsEmpty) ? container.GetSlotCount(targetSlot) : 0);
		int num3 = requiredCount;
		if (num != 0 && MatchesAny(num, accepted))
		{
			int slotCapacity = container.GetSlotCapacity(targetSlot, num);
			num3 = ((num2 < slotCapacity) ? ((num2 >= requiredCount) ? 1 : (requiredCount - num2)) : 0);
		}
		else if (num != 0 && options.FillEmptyOnly)
		{
			plan.Add(new SlotAction
			{
				MissingLabel = FormatMissingValues(accepted, requiredCount)
			});
			return;
		}
		if (num3 <= 0)
		{
			plan.Add(new SlotAction
			{
				TargetSlot = targetSlot
			});
			return;
		}
		if (!TryFindPlayerStackForValues(sources.PlayerInventory, accepted, num3, out var sourceSlot, out var blockValue, out var transferCount))
		{
			plan.Add(new SlotAction
			{
				MissingLabel = FormatMissingValues(accepted, num3)
			});
			return;
		}
		plan.Add(new SlotAction
		{
			TargetSlot = targetSlot,
			SourceSlot = sourceSlot,
			BlockValue = blockValue,
			Count = transferCount,
			NeedsTransfer = true
		});
	}

	private static PlacementResult ExecutePlan(IInventory container, PlacementSources sources, List<SlotAction> plan, bool execute)
	{
		if (!plan.Exists((SlotAction action) => action.NeedsTransfer) && !plan.Exists((SlotAction action) => action.MissingLabel != null))
		{
			return PlacementResult.AlreadySatisfied();
		}
		bool executedTransfer = false;
		List<string> list = new List<string>();
		if (execute)
		{
			foreach (SlotAction item in plan)
			{
				if (item.NeedsTransfer)
				{
					int num = sources.PlayerInventory.RemoveSlotItems(item.SourceSlot, item.Count);
					if (num <= 0)
					{
						list.Add(FormatMissingValue(item.BlockValue, item.Count));
					}
					else if (!CanAcceptSlotItems(container, item.TargetSlot, item.BlockValue, num))
					{
						sources.PlayerInventory.AddSlotItems(item.SourceSlot, item.BlockValue, num);
						list.Add(FormatMissingValue(item.BlockValue, num));
					}
					else
					{
						container.AddSlotItems(item.TargetSlot, item.BlockValue, num);
						executedTransfer = true;
					}
				}
			}
		}
		return ToResult(plan, execute, executedTransfer, list);
	}

	private static bool CanAcceptSlotItems(IInventory container, int slotIndex, int value, int count)
	{
		if (count <= 0)
		{
			return false;
		}
		int slotValue = container.GetSlotValue(slotIndex);
		int slotCount = container.GetSlotCount(slotIndex);
		if (slotCount > 0 && slotValue != value)
		{
			return false;
		}
		return slotCount + count <= container.GetSlotCapacity(slotIndex, value);
	}

	private static bool TryReturnMismatchedSlotContents(IInventory container, int slotIndex, string ingredient, int requiredCount, IInventory playerInventory, out string? error)
	{
		error = null;
		int slotValue = container.GetSlotValue(slotIndex);
		int num = container.GetSlotCount(slotIndex);
		if (num <= 0)
		{
			return true;
		}
		if (MatchesIngredient(ingredient, slotValue))
		{
			return true;
		}
		while (num > 0)
		{
			int num2 = TryAddToInventory(playerInventory, slotValue, num);
			if (num2 <= 0)
			{
				error = "背包已满，无法取出机器内原料";
				return false;
			}
			container.RemoveSlotItems(slotIndex, num2);
			num -= num2;
		}
		return true;
	}

	private static int TryAddToInventory(IInventory inventory, int value, int count)
	{
		int num = 0;
		int maxStacking = BlocksManager.Blocks[Terrain.ExtractContents(value)].GetMaxStacking(value);
		for (int i = 0; i < inventory.SlotsCount; i++)
		{
			if (inventory.GetSlotCapacity(i, value) == 0)
			{
				continue;
			}
			int slotCount = inventory.GetSlotCount(i);
			if (slotCount > 0 && inventory.GetSlotValue(i) != value)
			{
				continue;
			}
			int num2 = maxStacking - slotCount;
			if (num2 > 0)
			{
				int num3 = Math.Min(num2, count - num);
				inventory.AddSlotItems(i, value, num3);
				num += num3;
				if (num >= count)
				{
					break;
				}
			}
		}
		return num;
	}

	private static PlacementResult ToResult(List<SlotAction> plan, bool execute, bool executedTransfer, List<string> failedTransfers)
	{
		List<string> list = new List<string>();
		bool flag = execute && executedTransfer;
		foreach (SlotAction item in plan)
		{
			if (item.MissingLabel != null)
			{
				if (!list.Contains(item.MissingLabel))
				{
					list.Add(item.MissingLabel);
				}
			}
			else if (!execute && item.NeedsTransfer)
			{
				flag = true;
			}
		}
		foreach (string failedTransfer in failedTransfers)
		{
			if (!list.Contains(failedTransfer))
			{
				list.Add(failedTransfer);
			}
		}
		if (list.Count == 0)
		{
			return PlacementResult.Complete(flag);
		}
		if (flag)
		{
			return PlacementResult.Partial(list);
		}
		return PlacementResult.None(list);
	}

	private static bool TryFindPlayerStackForIngredient(IInventory inventory, string ingredient, int needed, out int sourceSlot, out int blockValue, out int transferCount)
	{
		sourceSlot = -1;
		blockValue = 0;
		transferCount = 0;
		int num = -1;
		for (int i = 0; i < inventory.SlotsCount; i++)
		{
			int slotValue = inventory.GetSlotValue(i);
			int slotCount = inventory.GetSlotCount(i);
			if (slotCount > 0 && MatchesIngredient(ingredient, slotValue) && slotCount > num)
			{
				num = slotCount;
				sourceSlot = i;
				blockValue = slotValue;
				transferCount = Math.Min(needed, slotCount);
			}
		}
		return sourceSlot >= 0;
	}

	private static bool TryFindPlayerStackForValues(IInventory inventory, int[] accepted, int needed, out int sourceSlot, out int blockValue, out int transferCount)
	{
		sourceSlot = -1;
		blockValue = 0;
		transferCount = 0;
		int num = int.MaxValue;
		int num2 = -1;
		for (int i = 0; i < inventory.SlotsCount; i++)
		{
			int slotValue = inventory.GetSlotValue(i);
			int slotCount = inventory.GetSlotCount(i);
			if (slotCount > 0)
			{
				int num3 = IndexOfAccepted(slotValue, accepted);
				if (num3 >= 0 && num3 <= num && (num3 != num || slotCount > num2))
				{
					num = num3;
					num2 = slotCount;
					sourceSlot = i;
					blockValue = slotValue;
					transferCount = Math.Min(needed, slotCount);
				}
			}
		}
		return sourceSlot >= 0;
	}

	private static int IndexOfAccepted(int blockValue, int[] accepted)
	{
		for (int i = 0; i < accepted.Length; i++)
		{
			if (accepted[i] == blockValue)
			{
				return i;
			}
		}
		return -1;
	}

	private static bool MatchesIngredient(string ingredient, int blockValue)
	{
		return RecipesUtils.CompareIngredients(ingredient, RecipesUtils.BlockToCratingID(blockValue));
	}

	private static bool MatchesAny(int blockValue, int[] accepted)
	{
		return IndexOfAccepted(blockValue, accepted) >= 0;
	}

	private static string FormatMissingIngredient(string ingredient, int count)
	{
		try
		{
			int num = RecipesUtils.DecodeIEResult(ingredient);
			if (IsValidBlockValue(num))
			{
				return FormatMissingValue(num, count);
			}
		}
		catch (Exception)
		{
		}
		int[] array = RecipesUtils.ExpandIngredientToBlockValues(ingredient);
		int[] array2 = array;
		foreach (int blockValue in array2)
		{
			if (MatchesIngredient(ingredient, blockValue))
			{
				return FormatMissingValue(blockValue, count);
			}
		}
		if (array.Length == 1 && IsValidBlockValue(array[0]))
		{
			return FormatMissingValue(array[0], count);
		}
		RecipesUtils.DecodeIEIngredient(ingredient, out string craftingId, out int? _);
		if (!string.IsNullOrEmpty(craftingId))
		{
			return "缺少 " + craftingId;
		}
		return "缺少原料";
	}

	private static bool IsValidBlockValue(int value)
	{
		if (value <= 0)
		{
			return false;
		}
		int num = Terrain.ExtractContents(value);
		if (num >= 0)
		{
			return num < BlocksManager.Blocks.Length;
		}
		return false;
	}

	private static string FormatMissingValues(int[] accepted, int count)
	{
		if (accepted.Length == 0)
		{
			return "缺少原料";
		}
		return FormatMissingValue(accepted[0], count);
	}

	private static string FormatMissingValue(int blockValue, int count)
	{
		string displayName = BlocksManager.Blocks[Terrain.ExtractContents(blockValue)].GetDisplayName(null, blockValue);
		if (count <= 1)
		{
			return "缺少 " + displayName;
		}
		return $"缺少 {displayName} ×{count}";
	}
}
