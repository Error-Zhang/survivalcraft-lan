namespace SCIENEW;

public class ComponentBlastFurnaceCastingBasin : ComponentCastingBasin
{
	public static int RequiredHeatLevel => -7;

	public override string ProcessType => "BlastCasting";

	protected override string CrafterKindName => "BlastFurnaceCastingBasin";

	public override float GetTankCapacity(int tankIndex, int value)
	{
		if (!(FluidsManager.GetFluid(value) is IMaterialBlock materialBlock))
		{
			return 0f;
		}
		if (CastingUtils.IsHeatMetal(materialBlock.GetMaterial()))
		{
			return 0f;
		}
		return base.GetTankCapacity(tankIndex, value);
	}
}
