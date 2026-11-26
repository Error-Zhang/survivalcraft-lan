using Engine;
using Game;

namespace SCIENEW;

public class BasicCircuitItem : MeshItem
{
	public BasicCircuitItem()
		: base("BasicCircuit", "Models/BasicCircuit", "BasicCircuit", IndustrialModLoader.IETexture, Color.White, Matrix.CreateTranslation(0f, 0.4f, 0f), Matrix.Identity)
	{
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, 1f, 0f);
	}
}
