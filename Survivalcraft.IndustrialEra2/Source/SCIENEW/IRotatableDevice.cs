namespace SCIENEW;

public interface IRotatableDevice
{
	int GetNextDirection(int value, bool reverse = false);
}
