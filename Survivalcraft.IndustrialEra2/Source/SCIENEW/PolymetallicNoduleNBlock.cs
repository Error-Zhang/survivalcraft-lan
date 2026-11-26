using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class PolymetallicNoduleNBlock : StoneTypeNBlock
{
	public PolymetallicNoduleNBlock()
		: base("PolymetallicNodule", IndustrialModLoader.IETexture, Color.White, 26, 80, 12)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return 28;
	}

	public override string GetCategory(int value)
	{
		return "Minerals";
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		if (toolLevel > 2)
		{
			for (int i = 0; i < 3; i++)
			{
				dropValues.Add(new BlockDropValue
				{
					Value = ItemsBlockManager.GetItemIdByMaterial<OreChunkItem>(OresDataBase.PolymetallicNoduleDropOre(new Game.Random())),
					Count = 1
				});
			}
			int num = new Game.Random().Int(1, 4);
			for (int j = 0; j < num; j++)
			{
				dropValues.Add(new BlockDropValue
				{
					Value = ExperienceBlock.Index,
					Count = 1
				});
			}
		}
	}
}
