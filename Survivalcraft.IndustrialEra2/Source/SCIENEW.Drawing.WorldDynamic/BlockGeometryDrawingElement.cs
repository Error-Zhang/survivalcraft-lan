using System;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions.GameLogic;

namespace SCIENEW.Drawing.WorldDynamic;

public sealed class BlockGeometryDrawingElement : DynamicMeshDrawingElement
{
	private readonly PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

	private readonly DrawBlockEnvironmentData m_environmentData = new DrawBlockEnvironmentData();

	private readonly Action<PrimitivesRenderer3D, DrawBlockEnvironmentData> m_drawAction;

	private readonly Func<DrawingMaterial> m_materialFactory;

	public BlockGeometryDrawingElement(Func<DrawingMaterial> materialFactory, Action<PrimitivesRenderer3D, DrawBlockEnvironmentData> drawAction)
		: base(materialFactory())
	{
		m_materialFactory = materialFactory;
		m_drawAction = drawAction;
	}

	public void Rebuild(SubsystemTerrain subsystemTerrain, Point3 position)
	{
		Clear();
		Material = m_materialFactory();
		m_environmentData.ModifyEnvironmentData(subsystemTerrain, position);
		m_primitivesRenderer.Clear();
		m_drawAction(m_primitivesRenderer, m_environmentData);
		AppendRendererBatches();
		m_primitivesRenderer.Clear();
		MarkGeometryDirty();
	}

	private void AppendRendererBatches()
	{
		foreach (FlatBatch3D flatBatch in m_primitivesRenderer.m_flatBatches)
		{
			if (flatBatch.TriangleVertices.Count != 0)
			{
				Material = Material with
				{
					Texture = IndustrialModLoader.PureWhiteTexture,
					SamplerState = SamplerState.PointWrap
				};
				Mesh.AppendFlatBatch(flatBatch);
			}
		}
		foreach (FontBatch3D fontBatch in m_primitivesRenderer.m_fontBatches)
		{
			if (fontBatch.TriangleVertices.Count != 0)
			{
				Material = Material with
				{
					Texture = fontBatch.Font.Texture,
					SamplerState = SamplerState.PointWrap
				};
				Mesh.AppendFontBatch(fontBatch);
			}
		}
		foreach (TexturedBatch3D texturedBatch in m_primitivesRenderer.m_texturedBatches)
		{
			if (texturedBatch.TriangleVertices.Count != 0)
			{
				Material = Material with
				{
					Texture = texturedBatch.Texture,
					SamplerState = texturedBatch.SamplerState
				};
				Mesh.AppendTexturedBatch(texturedBatch);
			}
		}
	}
}
