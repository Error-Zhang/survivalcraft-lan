using Game;

namespace SCIENEW;

public interface IPaintableBlock
{
	int? GetPaintColor();

	int Paint(SubsystemTerrain subsystemTerrain, int value, int? color);
}
