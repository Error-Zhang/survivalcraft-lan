using System;
using Engine;
using Game;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemMoltenIronBlockBehavior : SubsystemFocalorsFluidBlockBehavior, IUpdateable
{
	public System.Random m_random = new System.Random();

	public float m_soundVolume;

	public SubsystemFireBlockBehavior m_subsystemFireBlockBehavior;

	public SubsystemParticles m_subsystemParticles;

	public override int[] HandledBlocks => new int[1] { BlocksManager.GetBlockIndex<MoltenIronBlock>() };

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public SubsystemMoltenIronBlockBehavior()
		: base(BlocksManager.GetBlockIndex<MoltenIronBlock>())
	{
	}

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		m_subsystemFireBlockBehavior = Project.FindSubsystem<SubsystemFireBlockBehavior>(throwOnError: true);
		m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
	}

	public void Update(float dt)
	{
		if (SubsystemTime.PeriodicGameTimeEvent(0.5, 0.0))
		{
			SpreadFluid();
		}
	}

	public override void OnItemHarvested(int x, int y, int z, int blockValue, ref BlockDropValue dropValue, ref int newBlockValue)
	{
		if (y > 80 && SubsystemWeather.IsPlaceFrozen(SubsystemTerrain.Terrain.GetSeasonalTemperature(x, z), y))
		{
			dropValue.Value = Terrain.MakeBlockValue(IronBlock.Index);
		}
		else
		{
			base.OnItemHarvested(x, y, z, blockValue, ref dropValue, ref newBlockValue);
		}
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		base.OnNeighborBlockChanged(x, y, z, neighborX, neighborY, neighborZ);
		ApplyMagmaNeighborhoodEffect(neighborX, neighborY, neighborZ);
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		base.OnBlockAdded(value, oldValue, x, y, z);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					ApplyMagmaNeighborhoodEffect(x + i, y + j, z + k);
				}
			}
		}
	}

	protected void ApplyMagmaNeighborhoodEffect(int x, int y, int z)
	{
		m_subsystemFireBlockBehavior.SetCellOnFire(x, y, z, 1f);
		switch (SubsystemTerrain.Terrain.GetCellContents(x, y, z))
		{
		case 61:
		case 62:
			SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
			m_subsystemParticles.AddParticleSystem(new BurntDebrisParticleSystem(SubsystemTerrain, new Vector3((float)x + 0.5f, y + 1, (float)z + 0.5f)));
			break;
		case 8:
			SubsystemTerrain.ChangeCell(x, y, z, 2);
			m_subsystemParticles.AddParticleSystem(new BurntDebrisParticleSystem(SubsystemTerrain, new Vector3((float)x + 0.5f, y + 1, (float)z + 0.5f)));
			break;
		}
	}
}
