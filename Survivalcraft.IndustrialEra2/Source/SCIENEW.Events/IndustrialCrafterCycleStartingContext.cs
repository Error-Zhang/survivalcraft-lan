using Engine;
using Game;
using GameEntitySystem;
using RecipaediaEX;

namespace SCIENEW.Events;

public readonly struct IndustrialCrafterCycleStartingContext(Project project, IInventory inventory, Point3 position, string crafterKind, IRecipe? recipe)
{
	public Project Project { get; } = project;

	public IInventory Inventory { get; } = inventory;

	public Point3 Position { get; } = position;

	public string CrafterKind { get; } = crafterKind ?? string.Empty;

	public IRecipe? Recipe { get; } = recipe;
}
