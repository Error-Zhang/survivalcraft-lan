using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class AntitankSniperBlock : RifleBlock
{
	public new static int Index = 527;

	public new Texture2D m_texture;

	public override void Initialize()
	{
		Model model = ContentManager.Get<Model>("Models/AntitankSniper");
		m_texture = ContentManager.Get<Texture2D>("Textures/AntitankSniper");
		m_standaloneBlockMeshUnloaded = new BlockMesh();
		ReadOnlyList<ModelMeshPart> meshParts = model.FindMesh("AntitankSniper").MeshParts;
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("AntitankSniper").ParentBone);
		m_standaloneBlockMeshUnloaded.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMeshLoaded = new BlockMesh();
		meshParts = model.FindMesh("AntitankSniper").MeshParts;
		m_standaloneBlockMeshLoaded.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, GetLoadState2(Terrain.ExtractData(value)) ? m_standaloneBlockMeshLoaded : m_standaloneBlockMeshUnloaded, m_texture, color, 2f * size, ref matrix, environmentData);
	}

	public override bool IsSwapAnimationNeeded(int oldValue, int newValue)
	{
		if (Terrain.ExtractContents(oldValue) == Index)
		{
			return RifleBlock.SetHammerState(Terrain.ExtractData(newValue), state: true) != RifleBlock.SetHammerState(Terrain.ExtractData(oldValue), state: true);
		}
		return true;
	}

	public static bool GetLoadState2(int data)
	{
		return data != ((data & -4) | 0);
	}

	public static int GetBulletNum(int data)
	{
		return (data >> 4) & 0x3F;
	}

	public static int SetBulletNum(int data)
	{
		return (data & -64) | ((data & 0x3F) << 4);
	}
}
