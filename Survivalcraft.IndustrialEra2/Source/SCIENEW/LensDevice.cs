using System;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class LensDevice : SixDirectionModelDevice, IEntityDevice<ComponentLens>
{
	public BlockMesh m_lensMesh = new BlockMesh();

	public LensDevice()
		: base("LensDevice", "Models/Lens", "Bottom", faceTransparent: true)
	{
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("Lens").ParentBone);
		m_lensMesh.AppendModelMeshPart(m_model.FindMesh("Lens").MeshParts[0], boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_lensMesh.TransformPositions(Matrix.CreateRotationY((float)Math.PI));
	}

	public override bool IsTransparent_(int value)
	{
		return true;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 99;
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		BlockPlacementData result = new BlockPlacementData();
		result.CellFace = raycastResult.CellFace;
		result.Value = DevicesBlockManager.GetDeviceValue(Id, raycastResult.CellFace.Face);
		return result;
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		base.DrawBlock(primitivesRenderer, value, color, size, ref matrix, environmentData);
		Matrix matrix2 = ((environmentData.DrawBlockMode == DrawBlockMode.UI) ? (Matrix.CreateTranslation(0f, -0.5f, 0f) * matrix) : (Matrix.CreateTranslation(0f, -0.15f, 0f) * matrix));
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_lensMesh, IndustrialModLoader.IETexture, color, size, ref matrix2, environmentData);
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new LensDeviceBehavior(project)
		};
	}

	public string GetEntityName(int value)
	{
		return "Lens";
	}

	public Widget GetWidget(IInventory inventory, ComponentLens component)
	{
		return new LensWidget(inventory, component);
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override float GetObjectShadowStrength(int value)
	{
		return 0f;
	}
}
