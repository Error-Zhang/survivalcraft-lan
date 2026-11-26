using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.Graphics;
using SCIENEW.Utils;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentIEModelDiggingCracks : Component, IDrawable
{
	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemSky m_subsystemSky;

	public ComponentMiner m_componentMiner;

	public Texture2D[] m_textures;

	public ModelShader m_shader;

	public Point3 m_point;

	public Dictionary<Model, InstancedModelData> m_cache = new Dictionary<Model, InstancedModelData>();

	public int m_value;

	public static int[] m_drawOrders = new int[1] { 200 };

	public int[] DrawOrders => m_drawOrders;

	public void Draw(Camera camera, int drawOrder)
	{
		if (!m_componentMiner.DigCellFace.HasValue || !(m_componentMiner.DigProgress > 0f) || !(m_componentMiner.DigTime > 0.2f))
		{
			return;
		}
		Point3 point = m_componentMiner.DigCellFace.Value.Point;
		int cellValue = m_subsystemTerrain.Terrain.GetCellValue(point.X, point.Y, point.Z);
		Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)];
		if (!BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, point, out ComponentBlockEntity blockEntity))
		{
			return;
		}
		ComponentIEModel componentIEModel = blockEntity.Entity.FindComponent<ComponentIEModel>();
		if (componentIEModel == null)
		{
			return;
		}
		if (cellValue != m_value || point != m_point)
		{
			m_point = point;
			m_value = cellValue;
		}
		InstancedModelData instancedModelData = GetInstancedModelData(componentIEModel.Model, componentIEModel.MeshDrawOrders);
		Vector3 translation = camera.InvertedViewMatrix.Translation;
		_ = Matrix.CreateTranslation(new Vector3(MathF.Floor(translation.X), 0f, MathF.Floor(translation.Z)) - translation) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
		SubsystemModelsRenderer.ModelData modelData = new SubsystemModelsRenderer.ModelData
		{
			ComponentModel = componentIEModel,
			ComponentBody = componentIEModel.Entity.FindComponent<ComponentBody>(),
			Light = m_subsystemSky.SkyLightIntensity
		};
		try
		{
			Display.BlendState = BlendState.NonPremultiplied;
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
			m_shader.PrepareModelShaderState(camera, m_subsystemSky, modelData, block.GetDiggingCrackingTexture(m_componentMiner, m_componentMiner.m_digProgress, cellValue, m_textures), 0f);
			Display.DrawIndexed(PrimitiveType.TriangleList, m_shader, instancedModelData.VertexBuffer, instancedModelData.IndexBuffer, 0, instancedModelData.IndexBuffer.IndicesCount);
		}
		catch
		{
		}
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemSky = Project.FindSubsystem<SubsystemSky>(throwOnError: true);
		m_componentMiner = Entity.FindComponent<ComponentMiner>(throwOnError: true);
		m_shader = new ModelShader(ShaderCodeManager.GetFast("Shaders/Model.vsh"), ShaderCodeManager.GetFast("Shaders/Model.psh"), useAlphaThreshold: true, Project.FindSubsystem<SubsystemModelsRenderer>().MaxInstancesCount);
		m_textures = new Texture2D[8];
		for (int i = 0; i < 8; i++)
		{
			m_textures[i] = ContentManager.Get<Texture2D>($"Textures/Cracks{i + 1}");
		}
	}

	public InstancedModelData GetInstancedModelData(Model model, int[] meshDrawOrders)
	{
		if (!m_cache.TryGetValue(model, out InstancedModelData value))
		{
			value = CreateInstancedModelData(model, meshDrawOrders);
			m_cache.Add(model, value);
		}
		return value;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(ValuesDictionary, entityToIdMap);
		foreach (InstancedModelData value in m_cache.Values)
		{
			value.VertexBuffer.Dispose();
			value.IndexBuffer.Dispose();
		}
		m_cache.Clear();
	}

	public static InstancedModelData CreateInstancedModelData(Model model, int[] meshDrawOrders)
	{
		DynamicArray<InstancedModelsManager.InstancedVertex> dynamicArray = new DynamicArray<InstancedModelsManager.InstancedVertex>();
		DynamicArray<int> dynamicArray2 = new DynamicArray<int>();
		for (int i = 0; i < meshDrawOrders.Length; i++)
		{
			ModelMesh modelMesh = model.Meshes[meshDrawOrders[i]];
			foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
			{
				_ = dynamicArray.Count;
				VertexBuffer vertexBuffer = meshPart.VertexBuffer;
				IndexBuffer indexBuffer = meshPart.IndexBuffer;
				ReadOnlyList<VertexElement> vertexElements = vertexBuffer.VertexDeclaration.VertexElements;
				int[] indexData = BlockMesh.GetIndexData<int>(indexBuffer);
				Dictionary<int, int> dictionary = new Dictionary<int, int>();
				if (vertexElements.Count != 3 || vertexElements[0].Offset != 0 || vertexElements[0].Semantic != VertexElementSemantic.Position.GetSemanticString() || vertexElements[1].Offset != 12 || vertexElements[1].Semantic != VertexElementSemantic.Normal.GetSemanticString() || vertexElements[2].Offset != 24 || vertexElements[2].Semantic != VertexElementSemantic.TextureCoordinate.GetSemanticString())
				{
					throw new InvalidOperationException("Unsupported vertex format.");
				}
				InstancedModelsManager.SourceModelVertex[] vertexData = BlockMesh.GetVertexData<InstancedModelsManager.SourceModelVertex>(vertexBuffer);
				for (int j = meshPart.StartIndex; j < meshPart.StartIndex + meshPart.IndicesCount; j++)
				{
					int num = indexData[j];
					if (!dictionary.ContainsKey(num))
					{
						dictionary.Add(num, dynamicArray.Count);
						InstancedModelsManager.InstancedVertex item = default;
						InstancedModelsManager.SourceModelVertex sourceModelVertex = vertexData[num];
						item.X = sourceModelVertex.X;
						item.Y = sourceModelVertex.Y;
						item.Z = sourceModelVertex.Z;
						item.Nx = sourceModelVertex.Nx;
						item.Ny = sourceModelVertex.Ny;
						item.Nz = sourceModelVertex.Nz;
						item.Tx = sourceModelVertex.Tx * 16f % 1f;
						item.Ty = sourceModelVertex.Ty * 16f % 1f;
						item.Instance = modelMesh.ParentBone.Index;
						dynamicArray.Add(item);
					}
				}
				for (int k = 0; k < meshPart.IndicesCount / 3; k++)
				{
					dynamicArray2.Add(dictionary[indexData[meshPart.StartIndex + 3 * k]]);
					dynamicArray2.Add(dictionary[indexData[meshPart.StartIndex + 3 * k + 1]]);
					dynamicArray2.Add(dictionary[indexData[meshPart.StartIndex + 3 * k + 2]]);
				}
			}
		}
		InstancedModelData instancedModelData = new InstancedModelData();
		instancedModelData.VertexBuffer = new VertexBuffer(InstancedModelData.VertexDeclaration, dynamicArray.Count);
		instancedModelData.IndexBuffer = new IndexBuffer(IndexFormat.ThirtyTwoBits, dynamicArray2.Count);
		instancedModelData.VertexBuffer.SetData(dynamicArray.Array, 0, dynamicArray.Count);
		instancedModelData.IndexBuffer.SetData(dynamicArray2.Array, 0, dynamicArray2.Count);
		return instancedModelData;
	}
}
