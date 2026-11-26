using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW.Screens;

public interface IScreenContainerComponent
{
	Screen[] GetScreens(Camera camera);

	Entity GetEntity();

	Vector3 GetPosition();

	BoundingBox GetBoundingBox();

	string GetDescription();
}
