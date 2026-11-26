namespace SCIENEW;

public class ComponentGasFurnaceTank : ComponentTankBase
{
	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (FluidsManager.GetFluid(value) is IFuelFluid fuelFluid && fuelFluid.GetFuelLevel(value) > 0f)
		{
			return GetTankOriginalCapacity(tankIndex);
		}
		return 0f;
	}

	public override void AddTankFluid(int tankIndex, int value, float volume, out float overflow)
	{
		base.AddTankFluid(tankIndex, value, volume, out overflow);
		Entity.FindComponent<ComponentGasFurnace>()?.SetFuelStateDirty();
	}

	public override float RemoveTankFluid(int tankIndex, float volume)
	{
		float num = base.RemoveTankFluid(tankIndex, volume);
		if (num > 0f)
		{
			ComponentGasFurnace componentGasFurnace = Entity.FindComponent<ComponentGasFurnace>();
			if (componentGasFurnace == null)
			{
				return num;
			}
			componentGasFurnace.SetFuelStateDirty();
		}
		return num;
	}
}
