using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Drawing.WorldDynamic;
using SCIENEW.Extensions;
using SCIENEW.Extensions.Graphics;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemDrawingElements : Subsystem, IDrawable
{
	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemSky m_subsystemSky;

	public SubsystemShadows m_subsystemShadows;

	public SubsystemTimeOfDay m_subsystemTimeOfDay;

	private ModelShader m_shaderOpaque;

	private ModelShader m_shaderAlphaTested;

	private DrawingLightSampler m_lightSampler;

	private readonly List<WorldDrawingElement> m_elements = new List<WorldDrawingElement>();

	private readonly List<WorldDrawingElement> m_elementsToPrepare = new List<WorldDrawingElement>();

	private readonly Dictionary<ModelRenderingMode, List<WorldDrawingElement>> m_elementsToDraw = new Dictionary<ModelRenderingMode, List<WorldDrawingElement>>();

	public int MaxInstancesCount { get; private set; }

	public int[] DrawOrders => new int[4] { -10000, 1, 99, 201 };

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		for (int i = 0; i < DrawOrders.Length; i++)
		{
			m_elementsToDraw.Add((ModelRenderingMode)i, new List<WorldDrawingElement>());
		}
		m_subsystemTimeOfDay = Project.FindSubsystem<SubsystemTimeOfDay>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemSky = Project.FindSubsystem<SubsystemSky>(throwOnError: true);
		m_subsystemShadows = Project.FindSubsystem<SubsystemShadows>(throwOnError: true);
		m_lightSampler = new DrawingLightSampler(m_subsystemTerrain);
		MaxInstancesCount = DrawingRenderCapacity.CalculateCurrentDeviceMaxInstances();
		ModsManager.HookAction("GetMaxInstancesCount", (ModLoader modLoader) =>
		{
			MaxInstancesCount = Math.Clamp(Math.Max(modLoader.GetMaxInstancesCount(), MaxInstancesCount), 1, 64);
			return false;
		});
		string fname = "Shaders/DrawingElements.vsh";
		string fname2 = "Shaders/DrawingElements.psh";
		m_shaderOpaque = new ModelShader(ShaderCodeManager.GetFast(fname), ShaderCodeManager.GetFast(fname2), useAlphaThreshold: false, MaxInstancesCount);
		m_shaderAlphaTested = new ModelShader(ShaderCodeManager.GetFast(fname), ShaderCodeManager.GetFast(fname2), useAlphaThreshold: true, MaxInstancesCount);
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (drawOrder == DrawOrders[0])
		{
			m_elementsToPrepare.Clear();
			foreach (KeyValuePair<ModelRenderingMode, List<WorldDrawingElement>> item in m_elementsToDraw)
			{
				item.Value.Clear();
			}
			foreach (WorldDrawingElement element in m_elements)
			{
				element.Preprocess(Project, camera);
				element.BakeIfNeeded();
				if (element.IsVisible(Project, camera, m_subsystemSky) && element.IndexBuffer != null && element.VertexBuffer != null)
				{
					element.UpdateLight(Project, m_lightSampler);
					element.UpdateCameraTransforms(camera);
					m_elementsToPrepare.Add(element);
				}
			}
			m_elementsToPrepare.Sort();
			{
				foreach (WorldDrawingElement item2 in m_elementsToPrepare)
				{
					m_elementsToDraw[item2.RenderingMode].Add(item2);
				}
				return;
			}
		}
		if (drawOrder == DrawOrders[1])
		{
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
			Display.BlendState = BlendState.Opaque;
			DrawElements(camera, m_elementsToDraw[ModelRenderingMode.Solid], null);
			Display.RasterizerState = RasterizerState.CullNoneScissor;
			DrawElements(camera, m_elementsToDraw[ModelRenderingMode.AlphaThreshold], 0f);
			Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
		}
		else if (drawOrder == DrawOrders[2])
		{
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = RasterizerState.CullNoneScissor;
			Display.BlendState = BlendState.AlphaBlend;
			DrawElements(camera, m_elementsToDraw[ModelRenderingMode.TransparentBeforeWater], null);
		}
		else if (drawOrder == DrawOrders[3])
		{
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = RasterizerState.CullNoneScissor;
			Display.BlendState = BlendState.AlphaBlend;
			DrawElements(camera, m_elementsToDraw[ModelRenderingMode.TransparentAfterWater], null);
		}
	}

	private void DrawElements(Camera camera, List<WorldDrawingElement> elements, float? alphaThreshold)
	{
		ModelShader modelShader = (alphaThreshold.HasValue ? m_shaderAlphaTested : m_shaderOpaque);
		modelShader.LightDirection1 = -LightingManager.DirectionToLight1.TransformNormal(camera.ViewMatrix);
		modelShader.LightDirection2 = -LightingManager.DirectionToLight2.TransformNormal(camera.ViewMatrix);
		modelShader.FogColor = new Vector3(m_subsystemSky.ViewFogColor);
		modelShader.PrepareModelShaderFog(m_subsystemSky, camera);
		modelShader.FogYMultiplier = m_subsystemSky.VisibilityRangeYMultiplier;
		modelShader.WorldUp = Vector3.UnitY.TransformNormal(camera.ViewMatrix);
		modelShader.Transforms.View = Matrix.Identity;
		modelShader.Transforms.Projection = camera.ProjectionMatrix;
		if (alphaThreshold.HasValue)
		{
			modelShader.AlphaThreshold = alphaThreshold.Value;
		}
		foreach (WorldDrawingElement element in elements)
		{
			DrawingMaterial material = element.Material;
			Matrix[] worldTransformsForCamera = element.WorldTransformsForCamera;
			modelShader.GetParameter("u_normalLightMixFrac").SetValue(material.UseVertexNormals ? 1f : 0f);
			modelShader.MaterialColor = new Vector4(material.Tint * material.Opacity, material.Opacity);
			modelShader.EmissionColor = material.EmissionColor;
			modelShader.AmbientLightColor = new Vector3(LightingManager.LightAmbient * element.Light);
			modelShader.DiffuseLightColor1 = new Vector3(element.Light);
			modelShader.DiffuseLightColor2 = new Vector3(element.Light);
			modelShader.Texture = material.Texture;
			modelShader.SamplerState = material.SamplerState;
			DrawElementInstances(modelShader, element, worldTransformsForCamera);
		}
	}

	private void DrawElementInstances(ModelShader modelShader, WorldDrawingElement element, Matrix[] transforms)
	{
		if (transforms.Length <= MaxInstancesCount)
		{
			modelShader.InstancesCount = transforms.Length;
			Array.Copy(transforms, modelShader.Transforms.World, transforms.Length);
			Display.DrawIndexed(PrimitiveType.TriangleList, modelShader, element.VertexBuffer, element.IndexBuffer, 0, element.IndexBuffer.IndicesCount);
		}
		else
		{
			if (!(element.VertexBuffer?.Tag is DynamicArray<VertexPNCTI> vertices) || !(element.IndexBuffer?.Tag is DynamicArray<int> indices))
			{
				return;
			}
			foreach (DrawingInstanceBatch item in DrawingInstanceBatcher.CreateBatches(vertices, indices, transforms.Length, MaxInstancesCount))
			{
				if (item.Vertices.Count == 0 || item.Indices.Count == 0)
				{
					continue;
				}
				using VertexBuffer vertexBuffer = new VertexBuffer(VertexPNCTI.VertexDeclaration, item.Vertices.Count);
				vertexBuffer.SetData(item.Vertices.Array, 0, item.Vertices.Count);
				using IndexBuffer indexBuffer = new IndexBuffer(IndexFormat.ThirtyTwoBits, item.Indices.Count);
				indexBuffer.SetData(item.Indices.Array, 0, item.Indices.Count);
				modelShader.InstancesCount = item.TransformCount;
				Array.Copy(transforms, item.TransformOffset, modelShader.Transforms.World, 0, item.TransformCount);
				Display.DrawIndexed(PrimitiveType.TriangleList, modelShader, vertexBuffer, indexBuffer, 0, indexBuffer.IndicesCount);
			}
		}
	}

	public void Register(WorldDrawingElement drawingElement)
	{
		if (!m_elements.Contains(drawingElement))
		{
			m_elements.Add(drawingElement);
		}
	}

	public void Register(IEnumerable<WorldDrawingElement> drawingElements)
	{
		foreach (WorldDrawingElement drawingElement in drawingElements)
		{
			Register(drawingElement);
		}
	}

	public bool Unregister(WorldDrawingElement drawingElement, bool dispose = true)
	{
		bool flag = m_elements.Remove(drawingElement);
		if (flag & dispose)
		{
			drawingElement.Dispose();
		}
		return flag;
	}

	public void Unregister(IEnumerable<WorldDrawingElement> drawingElements, bool dispose = true)
	{
		foreach (WorldDrawingElement drawingElement in drawingElements)
		{
			Unregister(drawingElement, dispose);
		}
	}
}
