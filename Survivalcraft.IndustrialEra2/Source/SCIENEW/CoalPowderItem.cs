using Engine;

namespace SCIENEW;

public class CoalPowderItem : PowderItem
{
	public readonly float HeatLevel;

	public readonly float FuelFireDuration;

	public readonly float ExplosivePreesure;

	public CoalPowderItem(string id, Color color, float heatLevel, float fuelFireDuration, float explosivepreesure = 0f)
		: base(id, color)
	{
		HeatLevel = heatLevel;
		FuelFireDuration = fuelFireDuration;
		ExplosivePreesure = explosivepreesure;
	}

	public override float GetFuelHeatLevel(int value)
	{
		return HeatLevel;
	}

	public override float GetExplosionPressure(int value)
	{
		return ExplosivePreesure;
	}

	public override float GetFuelFireDuration(int value)
	{
		return FuelFireDuration;
	}
}
