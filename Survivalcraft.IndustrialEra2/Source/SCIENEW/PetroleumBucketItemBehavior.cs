using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class PetroleumBucketItemBehavior : ItemBehavior
{
	public PetroleumBucketItemBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		return IndustrialFluidBucket.Pour(Project, componentMiner, ray, PetroleumBlock.Index);
	}
}
