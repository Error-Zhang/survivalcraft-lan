using System;
using System.Collections.Generic;
using Game;
using RecipaediaEX.ComponentsExtra.Implementation;
using SCIENEW.ProductionIO;
using SCIENEW.Utils;

namespace SCIENEW;

public class ComponentIECraftingTable : ComponentEXCraftingTable, IElectricElementCrafterComponent, IUpdateable, IInventoryProductionSlots
{
	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	public int? RequireItemSlotIndex { get; set; }

	public override string ToCraftingID(int slotValue)
	{
		return RecipesUtils.BlockToCratingID(slotValue);
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		return kind switch
		{
			ProductionSlotKind.Input => EnumerateInputSlots(),
			ProductionSlotKind.Output => new _003C_003Ez__ReadOnlyArray<int>(new int[2] { ResultSlotIndex, RemainsSlotIndex }),
			_ => m_emptySlotIndices,
		};
	}

	private IEnumerable<int> EnumerateInputSlots()
	{
		for (int i = 0; i < SlotsCount - 2; i++)
		{
			yield return i;
		}
	}
}
