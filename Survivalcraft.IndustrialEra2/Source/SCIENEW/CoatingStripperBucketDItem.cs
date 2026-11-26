using Game;

namespace SCIENEW;

public class CoatingStripperBucketDItem : CoatingBucketDItem
{
	public CoatingStripperBucketDItem()
		: base(null)
	{
		Id = "CoatingStripperBucket";
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return LanguageControl.Get("DamageableBlocks", "Names", Id);
	}

	public override string GetDescription(int value)
	{
		return LanguageControl.Get("DamageableBlocks", "Descriptions", Id);
	}
}
