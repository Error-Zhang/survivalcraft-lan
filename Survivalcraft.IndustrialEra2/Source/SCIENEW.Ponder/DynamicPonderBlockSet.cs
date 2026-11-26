using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;

namespace SCIENEW.Ponder;

public class DynamicPonderBlockSet : IPonderBlockSet<DynamicPonderBlock>, IPonderSet
{
	protected List<DynamicPonderBlock> m_blocks = new List<DynamicPonderBlock>();

	protected Dictionary<DynamicPonderBlock, Block> m_map = new Dictionary<DynamicPonderBlock, Block>();

	public bool m_needsUpdate = true;

	public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	public DrawBlockEnvironmentData m_drawBlockEnvironmentData;

	public List<DynamicPonderBlock> PonderBlocks => m_blocks;

	public Ponder Ponder { get; set; }

	public DynamicPonderBlockSet(Project project, Ponder ponder)
	{
		m_drawBlockEnvironmentData = BlocksManager.m_defaultEnvironmentData;
		m_drawBlockEnvironmentData.DrawBlockMode = DrawBlockMode.ThirdPerson;
	}

	public DynamicPonderBlock Add(int value, Matrix matrix, Color color)
	{
		DynamicPonderBlock dynamicPonderBlock = new DynamicPonderBlock(value, matrix, color);
		m_blocks.Add(dynamicPonderBlock);
		m_needsUpdate = true;
		return dynamicPonderBlock;
	}

	public void Remove(DynamicPonderBlock dynamicPonderBlock)
	{
		m_blocks.Remove(dynamicPonderBlock);
		m_needsUpdate = true;
	}

	public void Draw(Camera camera)
	{
		foreach (KeyValuePair<DynamicPonderBlock, Block> item in m_map)
		{
			Matrix matrix = item.Key.Matrix;
			item.Value.DrawBlock(m_primitivesRenderer3D, item.Key.Value, item.Key.Color, 1f, ref matrix, BlocksManager.m_defaultEnvironmentData);
		}
		m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
	}

	public void Update()
	{
		if (!m_needsUpdate)
		{
			return;
		}
		m_needsUpdate = false;
		m_map.Clear();
		foreach (DynamicPonderBlock block in m_blocks)
		{
			Block value = BlocksManager.Blocks[Terrain.ExtractContents(block.Value)];
			m_map.Add(block, value);
		}
	}
}
