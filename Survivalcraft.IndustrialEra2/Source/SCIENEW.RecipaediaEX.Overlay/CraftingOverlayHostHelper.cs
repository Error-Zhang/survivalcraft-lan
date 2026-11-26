using Game;
using GameEntitySystem;
using RecipaediaEX.ComponentsExtra.Implementation;
using RecipaediaEX.Overlay;

namespace SCIENEW.RecipaediaEX.Overlay;

public static class CraftingOverlayHostHelper
{
	public static int GetCrafterBlockValue(Component component)
	{
		return component.Entity.FindComponent<ComponentBlockEntity>(throwOnError: false)?.BlockValue ?? 0;
	}

	public static RecipaediaCraftingContext BuildCraftingTableContext(ComponentEXCraftingTable table)
	{
		ComponentPlayer componentPlayer = table.FindInteractingPlayer();
		return new RecipaediaCraftingContext
		{
			CrafterBlockValue = GetCrafterBlockValue(table),
			PlayerLevel = (componentPlayer?.PlayerData.Level ?? 1f),
			Project = table.Project,
			Inventory = componentPlayer?.ComponentMiner.Inventory
		};
	}

	public static RecipaediaCraftingContext BuildReactorContext(ComponentReactor reactor)
	{
		ComponentPlayer componentPlayer = reactor.FindInteractingPlayer();
		return new RecipaediaCraftingContext
		{
			CrafterBlockValue = GetCrafterBlockValue(reactor),
			PlayerLevel = (componentPlayer?.PlayerData.Level ?? 1f),
			RequiredHeatLevel = ComponentReactor.RequiredHeatLevel,
			Project = reactor.Project,
			Inventory = componentPlayer?.ComponentMiner.Inventory
		};
	}

	public static RecipaediaCraftingContext BuildCrafterContext(Component component)
	{
		ComponentPlayer componentPlayer = ((component is ComponentInventoryBase componentInventoryBase) ? componentInventoryBase.FindInteractingPlayer() : component.Entity.FindComponent<ComponentPlayer>());
		return new RecipaediaCraftingContext
		{
			CrafterBlockValue = GetCrafterBlockValue(component),
			PlayerLevel = (componentPlayer?.PlayerData.Level ?? 1f),
			Project = component.Project,
			Inventory = componentPlayer?.ComponentMiner.Inventory
		};
	}
}
