using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class BeaconDevice : DefaultDevice
{
	public BlockMesh m_blockMesh = new BlockMesh();

	public BlockMesh m_standaloneBlockMesh = new BlockMesh();

	public Texture2D m_texture;

	public BoundingBox m_boundingBox;

	public BeaconDevice()
		: base("BeaconDevice")
	{
		Model model = ContentManager.Get<Model>("Models/Beacon");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Head").ParentBone);
		Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Body").ParentBone);
		m_blockMesh.AppendModelMeshPart(model.FindMesh("Body").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		m_blockMesh.AppendModelMeshPart(model.FindMesh("Head").MeshParts[0], boneAbsoluteTransform2 * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: true, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Body").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
		m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Head").MeshParts[0], boneAbsoluteTransform2 * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: true, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_texture = IndustrialModLoader.IETexture;
		m_boundingBox = m_blockMesh.CalculateBoundingBox();
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateShadedMeshVertices(BlocksManager.Blocks[BaseDeviceBlock.Index], x, y, z, m_blockMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetAlphaTest);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, m_texture, color, size, ref matrix, environmentData);
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return new BoundingBox[1] { m_boundingBox };
	}

	public override bool IsCollidable_(int value)
	{
		return false;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override int GetEmittedLightAmount(int value)
	{
		return 5;
	}

	public override int GetShadowStrength(int value)
	{
		return -99;
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return new Vector3(0f, 0.15f, 0f);
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 1.4f;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new BeaconDeviceBehavior(project)
		};
	}
}
