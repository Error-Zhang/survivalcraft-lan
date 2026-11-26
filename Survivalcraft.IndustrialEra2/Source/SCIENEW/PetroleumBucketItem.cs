using Engine;
using GameEntitySystem;

namespace SCIENEW;

public class PetroleumBucketItem : BucketItem
{
	public PetroleumBucketItem()
		: base("PetroleumBucket", Color.DarkGray, "Tools")
	{
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new PetroleumBucketItemBehavior(project)
		};
	}
}
