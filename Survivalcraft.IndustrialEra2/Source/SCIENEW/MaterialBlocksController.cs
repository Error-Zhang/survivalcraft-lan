using System;
using System.Collections.Generic;

namespace SCIENEW;

public static class MaterialBlocksController
{
	public static List<IMaterialBlock> m_materialBlocks = new List<IMaterialBlock>();

	public static HashSet<Type> m_materialBlockTypes = new HashSet<Type>();

	public static void Init()
	{
		ItemBlock[] items = ItemsBlockManager.Items;
		foreach (ItemBlock itemBlock in items)
		{
			if (itemBlock is IMaterialBlock item)
			{
				m_materialBlocks.Add(item);
				m_materialBlockTypes.Add(itemBlock.GetType());
			}
		}
		NormalBlock[] blocks = NormalBlockManager.Blocks;
		foreach (NormalBlock normalBlock in blocks)
		{
			if (normalBlock is IMaterialBlock item2)
			{
				m_materialBlocks.Add(item2);
				m_materialBlockTypes.Add(normalBlock.GetType());
			}
		}
	}
}
