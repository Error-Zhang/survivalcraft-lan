using Engine;

namespace SCIENEW;

public interface IFocus
{
	void Focus(Vector3 position);

	Vector3 GetFocus();

	void SetFocus(Vector3 focus);
}
