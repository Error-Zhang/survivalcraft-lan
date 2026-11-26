using System;
using System.Collections.Generic;
using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class FlaxNBlock : PlantNBlock
{
	public int[] DropValues = Array.Empty<int>();

	public FlaxNBlock(string id, Color color, int faceTextureSlot, int[] dropValues)
		: base(id, IndustrialModLoader.IETexture, color, faceTextureSlot)
	{
		DropValues = dropValues;
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		if (DropValues.Length != 0)
		{
			int[] dropValues2 = DropValues;
			foreach (int value in dropValues2)
			{
				dropValues.Add(new BlockDropValue
				{
					Value = value,
					Count = 1
				});
			}
		}
	}

	public override NormalBlockBehavior[] GetBehaviors(Project project)
	{
		return new NormalBlockBehavior[1]
		{
			new FlaxNBlockBehavior(project)
		};
	}
}
