using Engine;
using GameEntitySystem;

namespace SCIENEW;

public class MercuryBucketItem : BucketItem
{
	public MercuryBucketItem()
		: base("MercuryBucket", Color.White, "Tools")
	{
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new MercuryBucketItemBehavior(project)
		};
	}
}
