using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class ShotGunBlock : MusketBlock
{
	public new static int Index = 523;

	public Texture2D m_texture;

	public override void Initialize()
	{
		m_standaloneBlockMeshUnloaded = new BlockMesh();
		Model model = ContentManager.Get<Model>("Models/Shotgun");
		ReadOnlyList<ModelMeshPart> meshParts = model.FindMesh("Shotgun").MeshParts;
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Shotgun").ParentBone);
		m_standaloneBlockMeshUnloaded.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		meshParts = model.FindMesh("Hammer").MeshParts;
		Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Hammer").ParentBone);
		m_standaloneBlockMeshUnloaded.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMeshLoaded = new BlockMesh();
		meshParts = model.FindMesh("Shotgun").MeshParts;
		m_standaloneBlockMeshLoaded.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		meshParts = model.FindMesh("Hammer").MeshParts;
		m_standaloneBlockMeshLoaded.AppendModelMeshPart(meshParts[0], Matrix.CreateRotationX(0.7f) * boneAbsoluteTransform2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_texture = ContentManager.Get<Texture2D>("Textures/Shotgun1");
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, GetLoadState2(Terrain.ExtractData(value)) ? m_standaloneBlockMeshLoaded : m_standaloneBlockMeshUnloaded, m_texture, color, 2f * size, ref matrix, environmentData);
	}

	public override bool IsSwapAnimationNeeded(int oldValue, int newValue)
	{
		if (Terrain.ExtractContents(oldValue) == Index)
		{
			return MusketBlock.SetHammerState(Terrain.ExtractData(newValue), state: true) != MusketBlock.SetHammerState(Terrain.ExtractData(oldValue), state: true);
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
