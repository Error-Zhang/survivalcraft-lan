using Engine;
using Game;

namespace SCIENEW;

public class AdvancedCircuitItem : MeshItem
{
	public AdvancedCircuitItem()
		: base("AdvancedCircuit", "Models/BasicCircuit", "BasicCircuit", IndustrialModLoader.IETexture, Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.CreateTranslation(0f, -0.0625f, 0f))
	{
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, 1f, 0f);
	}
}
