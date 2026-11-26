namespace SCIENEW;

public class ComponentHearthFurnaceCastingBasin : ComponentCastingBasin
{
	public static int RequiredHeatLevel => -8;

	public override string ProcessType => "HearthCasting";

	protected override string CrafterKindName => "HearthFurnaceCastingBasin";

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (!(FluidsManager.GetFluid(value) is IMaterialBlock materialBlock))
		{
			return 0f;
		}
		if (!CastingUtils.IsHeatMetal(materialBlock.GetMaterial()))
		{
			return 0f;
		}
		return base.GetTankCapacity(tankIndex, value);
	}
}
