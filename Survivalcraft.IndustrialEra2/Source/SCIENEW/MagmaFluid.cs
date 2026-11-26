using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class MagmaFluid : Fluid, IScoopableFluid, IPlaceableFluid, ITroughTransportableFluid
{
	private Texture2D m_texture;

	public Texture2D Texture
	{
		get
		{
			if (m_texture == null)
			{
				m_texture = ContentManager.Get<Texture2D>("Textures/NormalMagma");
			}
			return m_texture;
		}
	}

	public BiDictionary<int, int> AcquireContainer(int value)
	{
		return new BiDictionary<int, int> {
		{
			EmptyBucketBlock.Index,
			MagmaBucketBlock.Index
		} };
	}

	public override string GetCraftingID(int value)
	{
		return "MagmaFluid";
	}

	public override Texture2D GetTexture(int value)
	{
		return Texture;
	}

	public bool IsScoopable(int container, out int bucketIndex)
	{
		bucketIndex = 0;
		if (container == EmptyBucketBlock.Index)
		{
			bucketIndex = MagmaBucketBlock.Index;
			return true;
		}
		return false;
	}

	public override int GetCongenericFluidCount()
	{
		return 1;
	}

	public int GetFluidBlockIndex(int value)
	{
		return MagmaBlock.Index;
	}

	public override bool IsGlow(int value)
	{
		return true;
	}

	public override void Emit(Project project, Vector3 position, Vector3 velocity)
	{
		project.FindSubsystem<SubsystemParticles>().AddParticleSystem(new MagmaSplashParticleSystem(project.FindSubsystem<SubsystemTerrain>(), position, large: false));
		project.FindSubsystem<SubsystemAudio>().PlayRandomSound("Audio/Sizzles", 1f, new Game.Random().Float(-0.2f, 0.2f), position, 3f, autoDelay: true);
	}
}
