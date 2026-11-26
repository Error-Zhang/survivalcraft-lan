using System.Collections.Generic;

namespace SCIENEW.VoltNet;

public interface IVoltWireDevice
{
	HashSet<int> GetFaceMask(int value);

	float GetPL(int value);

	float GetUL(int value);

	Materials GetMaterial();
}
