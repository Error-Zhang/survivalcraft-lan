using Game;
using GameEntitySystem;

namespace SCIENEW;

public interface IEntityDevice<T> where T : Component
{
	string GetEntityName(int value);

	Widget GetWidget(IInventory inventory, T component);
}
