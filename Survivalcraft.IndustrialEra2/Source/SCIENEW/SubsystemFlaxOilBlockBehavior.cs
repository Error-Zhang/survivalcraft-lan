using System;
using Game;

namespace SCIENEW;

public class SubsystemFlaxOilBlockBehavior : SubsystemFluidBlockBehavior, IUpdateable
{
	public System.Random m_random = new System.Random();

	public float m_soundVolume;

	public override int[] HandledBlocks => new int[1] { BlocksManager.GetBlockIndex<FlaxOilBlock>() };

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public SubsystemFlaxOilBlockBehavior()
		: base(BlocksManager.FluidBlocks[FlaxOilBlock.Index], generateSources: false)
	{
	}

	public void Update(float dt)
	{
		if (SubsystemTime.PeriodicGameTimeEvent(1.0, 0.0))
		{
			SpreadFluid();
		}
	}

	public override bool OnFluidInteract(int interactValue, int x, int y, int z, int fluidValue)
	{
		if (BlocksManager.Blocks[Terrain.ExtractContents(interactValue)] is MagmaBlock)
		{
			SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
			Set(x, y, z, BasaltBlock.Index);
			return true;
		}
		return base.OnFluidInteract(interactValue, x, y, z, fluidValue);
	}
}
