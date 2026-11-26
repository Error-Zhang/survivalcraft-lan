using System.Collections.Generic;
using Engine;
using Game;

namespace SCIENEW;

public class MagnetiteNBlock : StoneTypeNBlock
{
	public MagnetiteNBlock()
		: base("Magnetite", IndustrialModLoader.IETexture, Color.White, 25, 80, 12)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return 26;
	}

	public override string GetCategory(int value)
	{
		return "Minerals";
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		Terrain.ExtractData(oldValue);
		if (toolLevel > 2)
		{
			if ((new Game.Random().Int() & 0xF) == 0)
			{
				dropValues.Add(new BlockDropValue
				{
					Value = ItemsBlockManager.IdMap["ScrapIron"],
					Count = 1
				});
			}
			else
			{
				int num = new Game.Random().Int(1, 2);
				int itemID = ItemsBlockManager.GetItemID("MagnetiteChunk");
				for (int i = 0; i < num; i++)
				{
					dropValues.Add(new BlockDropValue
					{
						Value = itemID,
						Count = 1
					});
				}
			}
			int num2 = new Game.Random().Int(1, 4);
			for (int j = 0; j < num2; j++)
			{
				dropValues.Add(new BlockDropValue
				{
					Value = ExperienceBlock.Index,
					Count = 1
				});
			}
			showDebris = true;
		}
		showDebris = true;
	}
}
