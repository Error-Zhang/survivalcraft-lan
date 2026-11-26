using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW.Screens;

public interface IScreenProviderComponent
{
	void Draw(Screen screen, Project project, Camera camera, int drawOrder);

	Entity GetEntity();

	Vector3 GetPosition();

	BoundingBox GetBoundingBox();

	string GetDescription();
}
