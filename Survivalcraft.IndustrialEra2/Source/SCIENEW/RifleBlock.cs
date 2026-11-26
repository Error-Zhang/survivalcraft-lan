using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class RifleBlock : FlatBlock
{
	public enum LoadState
	{
		Empty,
		Bullet,
		Bullet2,
		Loaded
	}

	public static int Index = 518;

	protected BlockMesh m_standaloneBlockMeshLoaded;

	protected BlockMesh m_standaloneBlockMeshUnloaded;

	protected Texture2D m_texture;

	public override void Initialize()
	{
		Model model = ContentManager.Get<Model>("Models/RifleMusket");
		m_texture = ContentManager.Get<Texture2D>("Textures/RifleMusket");
		m_standaloneBlockMeshUnloaded = new BlockMesh();
		ReadOnlyList<ModelMeshPart> meshParts = model.FindMesh("Musket").MeshParts;
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Musket").ParentBone);
		m_standaloneBlockMeshUnloaded.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		meshParts = model.FindMesh("Hammer").MeshParts;
		Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Hammer").ParentBone);
		m_standaloneBlockMeshUnloaded.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMeshLoaded = new BlockMesh();
		meshParts = model.FindMesh("Musket").MeshParts;
		m_standaloneBlockMeshLoaded.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		meshParts = model.FindMesh("Hammer").MeshParts;
		m_standaloneBlockMeshLoaded.AppendModelMeshPart(meshParts[0], Matrix.CreateRotationX(0.7f) * boneAbsoluteTransform2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		base.Initialize();
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, GetHammerState(Terrain.ExtractData(value)) ? m_standaloneBlockMeshLoaded : m_standaloneBlockMeshUnloaded, m_texture, color, 2f * size, ref matrix, environmentData);
	}

	public override bool IsSwapAnimationNeeded(int oldValue, int newValue)
	{
		if (Terrain.ExtractContents(oldValue) == Index)
		{
			return SetHammerState(Terrain.ExtractData(newValue), state: true) != SetHammerState(Terrain.ExtractData(oldValue), state: true);
		}
		return true;
	}

	public override int GetDamage(int value)
	{
		return (Terrain.ExtractData(value) >> 10) & 0xFF;
	}

	public override int SetDamage(int value, int damage)
	{
		return Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -261121) | ((damage & 0xFF) << 10));
	}

	public static LoadState GetLoadState(int data)
	{
		return (LoadState)(data & 3);
	}

	public static int SetLoadState(int data, LoadState loadState)
	{
		return (data & -4) | (int)(loadState & LoadState.Loaded);
	}

	public static bool GetHammerState(int data)
	{
		return (data & 4) != 0;
	}

	public static int SetHammerState(int data, bool state)
	{
		return (data & -5) | (int)((state ? 1u : 0u) << 2);
	}

	public override bool IsAimable_(int value)
	{
		return true;
	}
}
