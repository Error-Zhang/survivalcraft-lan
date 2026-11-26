using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class ScrapIronItem : MeshItem, IMaterialBlock
{
	public ScrapIronItem()
		: base("ScrapIron")
	{
		Model model = ContentManager.Get<Model>("Models/Campfire");
		m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Ashes").MeshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Ashes").ParentBone) * Matrix.CreateScale(3f), makeEmissive: false, flipWindingOrder: false, doubleSided: true, flipNormals: false, Color.White);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, size * 2f, ref matrix, environmentData);
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.Value = value;
		result.CellFace = raycastResult.CellFace;
		return result;
	}

	public Materials GetMaterial()
	{
		return Materials.Iron;
	}

	public float GetMoltedVolume()
	{
		return 0f;
	}
}
