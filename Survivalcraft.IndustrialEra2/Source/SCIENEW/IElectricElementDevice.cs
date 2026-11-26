using Game;

namespace SCIENEW;

public interface IElectricElementDevice
{
	ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z);

	ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z);

	int GetConnectionMask(int value);
}
