using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class FlaxOilBucketItemBehavior : ItemBehavior
{
	public FlaxOilBucketItemBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		return IndustrialFluidBucket.Pour(Project, componentMiner, ray, FlaxOilBlock.Index);
	}
}
