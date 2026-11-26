using System;
using Game;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemPetroleumBlockBehavior : SubsystemFluidBlockBehavior, IUpdateable
{
	public System.Random m_random = new System.Random();

	public float m_soundVolume;

	public int m_sludgeBlockID;

	public override int[] HandledBlocks => new int[1] { BlocksManager.GetBlockIndex<PetroleumBlock>() };

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public SubsystemPetroleumBlockBehavior()
		: base(BlocksManager.FluidBlocks[BlocksManager.GetBlockIndex<PetroleumBlock>()], generateSources: false)
	{
	}

	public void Update(float dt)
	{
		if (SubsystemTime.PeriodicGameTimeEvent(2.0, 0.0))
		{
			SpreadFluid();
		}
	}

	public override bool OnFluidInteract(int interactValue, int x, int y, int z, int fluidValue)
	{
		Block block = BlocksManager.Blocks[Terrain.ExtractContents(interactValue)];
		if ((block is WaterBlock || block is FlaxOilBlock) ? true : false)
		{
			SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
			Set(x, y, z, m_sludgeBlockID);
			return true;
		}
		if (BlocksManager.Blocks[Terrain.ExtractContents(interactValue)] is MagmaBlock)
		{
			SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
			Set(x, y, z, BasaltBlock.Index);
			return true;
		}
		return base.OnFluidInteract(interactValue, x, y, z, fluidValue);
	}

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_sludgeBlockID = NormalBlockManager.GetNormalBlockID("OilSludge");
	}
}
