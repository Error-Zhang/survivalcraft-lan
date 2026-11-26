using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class WaterFluid : Fluid, IScoopableFluid, IPlaceableFluid, IPipeableFluid
{
	public BiDictionary<int, int> AcquireContainer(int value)
	{
		return new BiDictionary<int, int> {
		{
			EmptyBucketBlock.Index,
			WaterBucketBlock.Index
		} };
	}

	public override string GetCraftingID(int value)
	{
		return "WaterFluid";
	}

	public bool IsScoopable(int container, out int bucketIndex)
	{
		bucketIndex = 0;
		if (container == EmptyBucketBlock.Index)
		{
			bucketIndex = WaterBucketBlock.Index;
			return true;
		}
		return false;
	}

	public override Color GetColor(int value)
	{
		return new Color(32, 80, 224, 255);
	}

	public override int GetCongenericFluidCount()
	{
		return 1;
	}

	public int GetFluidBlockIndex(int value)
	{
		return WaterBlock.Index;
	}

	public override void Emit(Project project, Vector3 position, Vector3 velocity)
	{
		project.FindSubsystem<SubsystemParticles>().AddParticleSystem(new WaterSplashParticleSystem(project.FindSubsystem<SubsystemTerrain>(), position, large: false));
		project.FindSubsystem<SubsystemAudio>().PlayRandomSound("Audio/Splashes", 1f, new Game.Random().Float(-0.2f, 0.2f), position, 6f, autoDelay: true);
	}
}
