using System;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CarriageItem : MeshItem
{
	public CarriageItem()
		: base("Minecart", "Models/Minecart", "obj1", ContentManager.Get<Texture2D>("Textures/Creatures/Jaguar"), Color.White, Matrix.CreateRotationZ(-(float)Math.PI / 2f), Matrix.Identity, 1f, 1.6f)
	{
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, Texture, color, size, ref matrix, environmentData);
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.6f, 0.8f, -0.8f);
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(-0.2f, -0.4f, 0f);
	}

	public override ItemBehavior[] GetBehaviors(Project project)
	{
		return new ItemBehavior[1]
		{
			new TrainItemBehavior(project)
		};
	}
}
