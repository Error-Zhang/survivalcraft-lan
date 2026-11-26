using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class SpringItem : MeshItem
{
	public SpringItem()
		: base("Spring")
	{
		m_standaloneBlockMesh = ItemsBlockManager.CreateRing();
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, IndustrialModLoader.BaseTexture, color, size, ref matrix, environmentData);
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, 0.1f, 0f);
	}
}
