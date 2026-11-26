using Engine;
using Engine.Graphics;

namespace SCIENEW;

public interface IRotorDItem
{
	Model GetModel();

	Matrix GetRotorTransformByFace(int direction);
}
