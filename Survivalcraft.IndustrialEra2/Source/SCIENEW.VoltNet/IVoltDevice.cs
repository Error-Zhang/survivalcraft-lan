using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW.VoltNet;

public interface IVoltDevice
{
	HashSet<int> GetFaceMask(int value);

	VoltElement? GetVoltElement(SubsystemVoltNet subsystemVoltNet, SubsystemTerrain subsystemTerrain, Point3 position, int blockValue);

	float GetStandardPL(int value);

	float GetStandardUL(int value);
}
