using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class MercuryBucketItemBehavior : ItemBehavior
{
	public MercuryBucketItemBehavior(Project project)
		: base(project)
	{
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		return IndustrialFluidBucket.Pour(Project, componentMiner, ray, MercuryBlock.Index);
	}
}
