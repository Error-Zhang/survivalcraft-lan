using System;
using Game;

namespace SCIENEW;

public class SubsystemMercuryBlockBehavior : SubsystemFocalorsFluidBlockBehavior, IUpdateable
{
	public System.Random m_random = new System.Random();

	public float m_soundVolume;

	public override int[] HandledBlocks => new int[1] { BlocksManager.GetBlockIndex<MercuryBlock>() };

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public SubsystemMercuryBlockBehavior()
		: base(BlocksManager.GetBlockIndex<MercuryBlock>())
	{
	}

	public void Update(float dt)
	{
		if (SubsystemTime.PeriodicGameTimeEvent(0.2, 0.0))
		{
			SpreadFluid();
		}
	}
}
