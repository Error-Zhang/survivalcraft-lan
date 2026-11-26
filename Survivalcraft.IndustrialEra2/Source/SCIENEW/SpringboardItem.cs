using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.Graphics;

namespace SCIENEW;

public class SpringboardItem : MeshItem
{
	protected BoundingBox[] m_collisionBoxes;

	private BlockMesh m_springMesh;

	public SpringboardItem()
		: base("Springboard")
	{
		m_springMesh = ItemsBlockManager.CreateRing();
		m_springMesh.TransformPositions(Matrix.CreateScale(0.9f) * Matrix.CreateTranslation(new Vector3(0.5f)));
		m_standaloneBlockMesh.AppendMesh("Models/Ingots", "IronPlate", Matrix.CreateTranslation(0.5f, 0.8f, 0.5f), Matrix.CreateTranslation(0.125f, 0f, 0f), Color.White);
		m_collisionBoxes = new BoundingBox[1] { m_standaloneBlockMesh.CalculateBoundingBox() };
	}

	public override string GetCraftingId()
	{
		return GetType().Name;
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return m_collisionBoxes;
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.Value = value;
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, IndustrialModLoader.IETexture, color, size, ref matrix, environmentData);
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_springMesh, IndustrialModLoader.BaseTexture, color, size, ref matrix, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
	{
		generator.GenerateMeshVertices(BlocksManager.Blocks[Terrain.ExtractContents(value)], x, y, z, m_standaloneBlockMesh, Color.White, null, geometry.GetGeometry(IndustrialModLoader.IETexture).SubsetOpaque);
		generator.GenerateMeshVertices(BlocksManager.Blocks[Terrain.ExtractContents(value)], x, y, z, m_springMesh, Color.White, null, geometry.GetGeometry(IndustrialModLoader.BaseTexture).SubsetOpaque);
	}

	public override bool IsPlaceable_(int value)
	{
		return true;
	}

	public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
	{
		return true;
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Metal";
	}

	public override int GetShadowStrength(int value)
	{
		return 12;
	}

	public override float GetDensity(int value)
	{
		return 3f;
	}
}
