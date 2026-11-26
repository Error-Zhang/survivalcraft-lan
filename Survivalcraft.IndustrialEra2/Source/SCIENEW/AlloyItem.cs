using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.Extensions.Graphics;
using SCIENEW.Utils;

namespace SCIENEW;

public class AlloyItem : MeshItem, IMaterialBlock
{
	public Materials Material;

	public Materials GetMaterial()
	{
		return Material;
	}

	public float GetMoltedVolume()
	{
		return 0f;
	}

	public AlloyItem(Materials type, string subId)
		: this(subId + "Alloy", (type == Materials.Copper) ? Color.White : type.GetColor(), type == Materials.Copper)
	{
		Material = type;
		DisplayName = string.Format(LanguageControl.Get("Items", "Names", "AlloyItem"), MaterialsUtils.GetAlloyDisplayName(subId));
		Description = string.Format(LanguageControl.Get("Items", "Descriptions", "AlloyItem"), MaterialsUtils.GetAlloyDisplayName(subId));
	}

	public AlloyItem(string id, Color color, bool isCopper = false)
		: base(id)
	{
		Material = Materials.None;
		Id = id;
		m_standaloneBlockMesh.AppendMesh("Models/Alloy", "Alloy", Matrix.CreateTranslation(0f, -0.1f, 0f), Matrix.Identity, color);
		Matrix matrix = (isCopper ? Matrix.CreateTranslation(0.1875f, 0.0625f, 0f) : Matrix.CreateTranslation(0.1875f, 0f, 0f));
		m_standaloneBlockMesh.TransformTextureCoordinates(matrix);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, IndustrialModLoader.IETexture, color, 1.6f * size, ref matrix, environmentData);
	}

	public override float GetMeleePower(int value)
	{
		return 2f;
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.65f;
	}
}
