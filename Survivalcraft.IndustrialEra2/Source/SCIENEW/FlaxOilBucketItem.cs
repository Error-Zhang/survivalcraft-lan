using Engine;
using GameEntitySystem;

namespace SCIENEW;

public class FlaxOilBucketItem : BucketItem
{
	public FlaxOilBucketItem()
		: base("FlaxOilBucket", new Color(255, 200, 0), "Tools")
	{
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new FlaxOilBucketItemBehavior(project)
		};
	}
}
