using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW.Ponder;

public class StaticPonderBlockSet : IPonderBlockSet<StaticPonderBlock>, IPonderSet, IDisposable
{
	public List<StaticPonderBlock> Blocks;

	public BlockGeometryGenerator? m_blockGeometryGenerator;

	public Terrain m_terrain;

	public DynamicArray<TerrainChunkGeometry.Buffer> Buffers;

	public TerrainGeometry Geometry;

	public Shader m_shader;

	public Project Project;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemSky m_subsystemSky;

	public SubsystemAnimatedTextures m_subsystemAnimatedTextures;

	public bool m_needsUpdate;

	List<StaticPonderBlock> IPonderBlockSet<StaticPonderBlock>.PonderBlocks => Blocks;

	public StaticPonderBlockSet(Project project, Ponder ponder)
	{
		Project = project;
		m_terrain = new Terrain();
		m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemSky = project.FindSubsystem<SubsystemSky>(throwOnError: true);
		m_subsystemAnimatedTextures = project.FindSubsystem<SubsystemAnimatedTextures>(throwOnError: true);
		Blocks = new List<StaticPonderBlock>();
		m_shader = new Shader(ShaderCodeManager.GetFast("Shaders/PonderDisplayer.vsh"), ShaderCodeManager.GetFast("Shaders/PonderDisplayer.psh"), new ShaderMacro("ALPHATESTED"));
		Buffers = new DynamicArray<TerrainChunkGeometry.Buffer>();
		m_needsUpdate = true;
		Geometry = new TerrainGeometry(m_subsystemAnimatedTextures.AnimatedBlocksTexture);
	}

	public void Dispose()
	{
		m_terrain.Dispose();
		Geometry.ClearGeometry();
	}

	public void SetStaticBlock(Point3 staticPosition, int value)
	{
		Blocks.RemoveAll((StaticPonderBlock b) => b.m_staticPosition == staticPosition);
		if (value != 0)
		{
			Blocks.Add(new StaticPonderBlock
			{
				m_staticPosition = staticPosition,
				m_value = value
			});
		}
		m_needsUpdate = true;
	}

	public void RemoveStaticBlock(Point3 staticPosition)
	{
		Blocks.RemoveAll((StaticPonderBlock b) => b.m_staticPosition == staticPosition);
		m_needsUpdate = true;
	}

	public void GenerateGeometry()
	{
		try
		{
			if (m_blockGeometryGenerator == null)
			{
				m_blockGeometryGenerator = new BlockGeometryGenerator(m_terrain, m_subsystemTerrain, null, Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(throwOnError: true), null, Project.FindSubsystem<SubsystemPalette>(throwOnError: true));
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						m_blockGeometryGenerator.Terrain.AllocateChunk(i, j);
					}
				}
			}
			TerrainChunk[] allocatedChunks = m_blockGeometryGenerator.Terrain.AllocatedChunks;
			for (int k = 0; k < allocatedChunks.Length; k++)
			{
				allocatedChunks[k].State = TerrainChunkState.Valid;
			}
			Point3 zero = Point3.Zero;
			Point3 point = new Point3(16, 255, 16);
			point.Y = MathUtils.Min(point.Y, 254);
			for (int l = zero.X - 1; l < point.X + 1; l++)
			{
				for (int m = zero.Y - 1; m < point.Z + 1; m++)
				{
					for (int n = zero.Z - 1; n < point.Y + 1; n++)
					{
						if (m_blockGeometryGenerator.Terrain.IsCellValid(l, n + zero.Y, m))
						{
							int value = Terrain.MakeBlockValue(ShadowBlock.Index, 15, 0);
							m_blockGeometryGenerator.Terrain.SetCellValueFast(l, n + zero.Y, m, value);
						}
					}
				}
			}
			foreach (StaticPonderBlock block in Blocks)
			{
				int x = block.m_staticPosition.X;
				int y = block.m_staticPosition.Y;
				int z = block.m_staticPosition.Z;
				if (m_blockGeometryGenerator.Terrain.IsCellValid(x, y + zero.Y, z))
				{
					int cellLightFast = m_blockGeometryGenerator.Terrain.GetCellLightFast(x, y + zero.Y, z);
					int value2 = Terrain.ReplaceLight(block.Value, cellLightFast);
					m_blockGeometryGenerator.Terrain.SetCellValueFast(x, y + zero.Y, z, value2);
				}
			}
			m_blockGeometryGenerator.ResetCache();
			Geometry.ClearGeometry();
			for (int num = 0; num < point.X; num++)
			{
				for (int num2 = 0; num2 < point.Y; num2++)
				{
					for (int num3 = 0; num3 < point.Z; num3++)
					{
						if (num2 + zero.Y > 0 && num2 + zero.Y < 255)
						{
							int cellValueFast = m_blockGeometryGenerator.Terrain.GetCellValueFast(num, num2 + zero.Y, num3);
							int num4 = Terrain.ExtractContents(cellValueFast);
							if (num4 != 0)
							{
								BlocksManager.Blocks[num4].GenerateTerrainVertices(m_blockGeometryGenerator, Geometry, cellValueFast, num, num2 + zero.Y, num3);
							}
						}
					}
				}
			}
		}
		catch (Exception value3)
		{
			Console.WriteLine(value3);
			throw;
		}
	}

	public void Draw(Camera camera)
	{
		foreach (TerrainChunkGeometry.Buffer buffer2 in Buffers)
		{
			buffer2.Dispose();
		}
		Buffers.Clear();
		if (m_needsUpdate)
		{
			GenerateGeometry();
			m_needsUpdate = false;
		}
		TerrainRenderer.CompileDrawSubsets(new TerrainGeometry[1] { Geometry }, Buffers, (TerrainVertex item) => item);
		for (int num = 0; num < Buffers.Count; num++)
		{
			TerrainChunkGeometry.Buffer buffer = Buffers[num];
			Display.BlendState = BlendState.AlphaBlend;
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
			m_shader.GetParameter("u_viewProjectionMatrix").SetValue(camera.ViewProjectionMatrix);
			m_shader.GetParameter("u_texture").SetValue(buffer.Texture);
			m_shader.GetParameter("u_samplerState").SetValue(SamplerState.PointClamp);
			m_shader.GetParameter("u_alphaThreshold").SetValue(0.5f);
			Display.DrawIndexed(PrimitiveType.TriangleList, m_shader, buffer.VertexBuffer, buffer.IndexBuffer, 0, buffer.IndexBuffer.IndicesCount);
		}
	}

	public void Update()
	{
	}
}
