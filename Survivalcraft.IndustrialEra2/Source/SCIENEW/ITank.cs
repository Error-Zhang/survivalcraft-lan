using Engine;
using GameEntitySystem;

namespace SCIENEW;

public interface ITank
{
	Project Project { get; }

	int TanksCount { get; }

	int VisibleTanksCount { get; set; }

	int GetTankValue(int tankIndex);

	float GetTankVolume(int tankIndex);

	float GetTankCapacity(int tankIndex, int value);

	float GetTankOriginalCapacity(int tankIndex);

	bool GetTankScoopable(int tankIndex, int container);

	void AddTankFluid(int tankIndex, int value, float volume, out float overflow);

	float RemoveTankFluid(int tankIndex, float volume);

	void EmitAllFluids(Vector3 position);
}
