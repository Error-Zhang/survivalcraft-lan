using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class MonitorLinkerItem : MeshItem
{
	public MonitorLinkerItem()
		: base("MonitorLinker", "Models/MonitorLinker", "Linker", IndustrialModLoader.IETexture, Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity, 2.4f)
	{
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new MonitorLinkerItemBehavior(project)
		};
	}

	public override string GetCategory(int value)
	{
		return "Tools";
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-1f, 0f, 0f);
	}
}
