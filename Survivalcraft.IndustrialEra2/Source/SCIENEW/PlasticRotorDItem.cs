using System;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class PlasticRotorDItem : MeshDamageableItem, IRotorDItem
{
	public BlockMesh[] m_blockMeshesByFace = new BlockMesh[6];

	public Model m_rotorModel;

	public Matrix[] m_rotorTransformByFace = new Matrix[6];

	public PlasticRotorDItem()
		: base("PlasticRotor", 1500, "Models/Rotor", "Rotor", IndustrialModLoader.IETexture, Color.White, Matrix.Identity, Matrix.Identity)
	{
		m_rotorModel = ContentManager.Get<Model>("Models/Rotor");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_rotorModel.FindMesh("Rotor").ParentBone);
		for (int i = 0; i < 6; i++)
		{
			m_rotorTransformByFace[i] = ((i >= 4) ? ((i != 4) ? (Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0f, 0f, 0f)) : Matrix.CreateTranslation(0f, 0f, 0f)) : (Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, 0f) * Matrix.CreateRotationY((float)i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, 0f)));
			m_blockMeshesByFace[i] = new BlockMesh();
			m_blockMeshesByFace[i].AppendModelMeshPart(m_rotorModel.FindMesh("Rotor").MeshParts[0], boneAbsoluteTransform * m_rotorTransformByFace[i], makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		}
	}

	public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return Vector3.UnitY;
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return 0.4f;
	}

	public Model GetModel()
	{
		return m_rotorModel;
	}

	public Matrix GetRotorTransformByFace(int direction)
	{
		return m_rotorTransformByFace[direction];
	}
}
