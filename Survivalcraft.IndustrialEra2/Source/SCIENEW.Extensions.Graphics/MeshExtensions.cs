using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW.Extensions.Graphics;

public static class MeshExtensions
{
	public static void AppendMesh(this BlockMesh blockMesh, string modelName, string meshName, Matrix boneTransform, Matrix tcTransform, Color color, bool doubleSided = false)
	{
		Model model = ContentManager.Get<Model>(modelName);
		blockMesh.AppendModelMeshPart(model.FindMesh(meshName).MeshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(meshName).ParentBone) * boneTransform, makeEmissive: false, flipWindingOrder: false, doubleSided, flipNormals: false, color);
		if (tcTransform != Matrix.Identity)
		{
			blockMesh.TransformTextureCoordinates(tcTransform);
		}
	}

	public static void AppendMeshWithNormal(this BlockMesh blockMesh, string modelName, string meshName, Matrix boneTransform, Matrix tcTransform, Color color)
	{
		Model model = ContentManager.Get<Model>(modelName);
		blockMesh.AppendModelMeshPartWithNormal(model.FindMesh(meshName).MeshParts[0], BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(meshName).ParentBone) * boneTransform, flipWindingOrder: false, doubleSided: false, flipNormals: false, color);
		if (tcTransform != Matrix.Identity)
		{
			blockMesh.TransformTextureCoordinates(tcTransform);
		}
	}

	public static void AppendModelMeshPartWithNormal(this BlockMesh blockMesh, ModelMeshPart meshPart, Matrix matrix, bool flipWindingOrder, bool doubleSided, bool flipNormals, Color color)
	{
		VertexBuffer vertexBuffer = meshPart.VertexBuffer;
		IndexBuffer indexBuffer = meshPart.IndexBuffer;
		ReadOnlyList<VertexElement> vertexElements = vertexBuffer.VertexDeclaration.VertexElements;
		if (vertexElements.Count != 3 || vertexElements[0].Offset != 0 || vertexElements[0].Semantic != VertexElementSemantic.Position.GetSemanticString() || vertexElements[1].Offset != 12 || vertexElements[1].Semantic != VertexElementSemantic.Normal.GetSemanticString() || vertexElements[2].Offset != 24 || vertexElements[2].Semantic != VertexElementSemantic.TextureCoordinate.GetSemanticString())
		{
			throw new InvalidOperationException("Wrong vertex format for a block mesh.");
		}
		BlockMesh.InternalVertex[] vertexData = BlockMesh.GetVertexData<BlockMesh.InternalVertex>(vertexBuffer);
		int[] indexData = BlockMesh.GetIndexData<int>(indexBuffer);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = meshPart.StartIndex; i < meshPart.StartIndex + meshPart.IndicesCount; i++)
		{
			int num = indexData[i];
			if (!dictionary.ContainsKey(num))
			{
				dictionary.Add(num, blockMesh.Vertices.Count);
				BlockMeshVertex item = default;
				item.Position = vertexData[num].Position.Transform(matrix);
				item.TextureCoordinates = vertexData[num].TextureCoordinate;
				Vector3 vector = (flipNormals ? (-vertexData[num].Normal) : vertexData[num].Normal).TransformNormal(matrix).Normalize();
				item.Color = new Color(vector);
				item.Color.A = color.A;
				item.Face = (byte)CellFace.Vector3ToFace(vector);
				blockMesh.Vertices.Add(item);
			}
		}
		for (int j = 0; j < meshPart.IndicesCount / 3; j++)
		{
			if (doubleSided)
			{
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j]]);
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j + 1]]);
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j + 2]]);
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j]]);
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j + 2]]);
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j + 1]]);
			}
			else if (flipWindingOrder)
			{
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j]]);
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j + 2]]);
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j + 1]]);
			}
			else
			{
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j]]);
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j + 1]]);
				blockMesh.Indices.Add(dictionary[indexData[meshPart.StartIndex + 3 * j + 2]]);
			}
		}
		blockMesh.Trim();
	}
}
