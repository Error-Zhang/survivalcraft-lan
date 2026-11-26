using Engine;
using Game;

namespace SCIENEW;

public interface ILaserHandler
{
	bool OnLaserHit(Vector3 renderStart, Vector3 incident, TerrainRaycastResult hit, Color color, double energy);
}
