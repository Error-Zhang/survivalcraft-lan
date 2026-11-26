using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.Graphics;

namespace SCIENEW;

public class BrickItem : MeshItem
{
	public Texture2D m_texture;

	public BrickItem(string id, Color color, Matrix tcTrarsform, Texture2D texture2D, float length = 1f)
		: base(id)
	{
		Id = id;
		m_texture = texture2D;
		m_standaloneBlockMesh.AppendMesh("Models/Brick", "Brick", Matrix.CreateScale(1f, 1f, length) * Matrix.CreateTranslation(0f, -0.08f, 0f) * 1.4f, tcTrarsform, color);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, m_texture, Color.White, 2f * size, ref matrix, environmentData);
	}

	public override float GetMeleePower(int value)
	{
		return 2f;
	}

	public override float GetProjectilePower(int value)
	{
		return 2f;
	}
}
