using Engine;
using Engine.Graphics;

namespace SCIENEW;

public class CoalBrickItem : BrickItem
{
	public readonly float HeatLevel;

	public readonly float FuelFireDuration;

	public readonly float ExplosivePreesure;

	public CoalBrickItem(string id, Color color, Matrix tcTrarsform, Texture2D texture2D, float heatLevel, float fuelFireDuration, float explosivepreesure = 0f)
		: base(id, color, tcTrarsform, texture2D, 1.25f)
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
