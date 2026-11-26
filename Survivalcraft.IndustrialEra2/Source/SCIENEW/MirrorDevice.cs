using System;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class MirrorDevice : SixDirectionModelDevice, IEntityDevice<ComponentMirror>
{
	public BlockMesh m_mirrorMesh = new BlockMesh();

	public MirrorDevice()
		: base("MirrorDevice", "Models/Mirror", "Bottom", faceTransparent: true)
	{
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_model.FindMesh("Mirror").ParentBone);
		m_mirrorMesh.AppendModelMeshPart(m_model.FindMesh("Mirror").MeshParts[0], boneAbsoluteTransform, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		m_mirrorMesh.TransformPositions(Matrix.CreateRotationY((float)Math.PI));
	}

	public override bool IsTransparent_(int value)
	{
		return true;
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
		Matrix matrix2 = Matrix.CreateTranslation(0f, -0.15f, 0f) * matrix;
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_mirrorMesh, IndustrialModLoader.IETexture, color, size, ref matrix2, environmentData);
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		return 99;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new MirrorDeviceBehavior(project)
		};
	}

	public string GetEntityName(int value)
	{
		return "Mirror";
	}

	public Widget GetWidget(IInventory inventory, ComponentMirror component)
	{
		return new MirrorWidget(inventory, component);
	}

	public override int GetShadowStrength(int value)
	{
		return 0;
	}

	public override float GetObjectShadowStrength(int value)
	{
		return 0f;
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return Block.m_defaultCollisionBoxes;
	}
}
