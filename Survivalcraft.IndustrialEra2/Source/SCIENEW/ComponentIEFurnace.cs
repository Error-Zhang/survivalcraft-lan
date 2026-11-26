using System;
using System.Collections.Generic;
using RecipaediaEX.ComponentsExtra.Implementation;
using SCIENEW.ProductionIO;

namespace SCIENEW;

public abstract class ComponentIEFurnace : ComponentEXFurnace, IInventoryProductionSlots
{
	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	protected override void UpdateFurnaceFireParticles(int cellValue)
	{
	}

	protected abstract override void ReplaceFurnaceBlock(int cellValue);

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		return kind switch
		{
			ProductionSlotKind.Input => EnumerateInputSlots(),
			ProductionSlotKind.Output => EnumerateOutputSlots(),
			_ => m_emptySlotIndices,
		};
	}

	private IEnumerable<int> EnumerateInputSlots()
	{
		int inputEnd = ResultSlotIndex;
		for (int i = 0; i < inputEnd; i++)
		{
			yield return i;
		}
	}

	private IEnumerable<int> EnumerateOutputSlots()
	{
		yield return ResultSlotIndex;
		if (RemainsSlotIndex >= 0 && RemainsSlotIndex < SlotsCount)
		{
			yield return RemainsSlotIndex;
		}
	}
}
