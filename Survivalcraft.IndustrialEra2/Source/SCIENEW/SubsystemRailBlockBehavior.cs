using Engine;
using Game;

namespace SCIENEW;

public class SubsystemRailBlockBehavior : SubsystemBlockBehavior
{
	protected Point3[,] m_pointsToUpdate = new Point3[4, 3]
	{
		{
			new Point3(0, 0, -1),
			new Point3(0, 1, -1),
			new Point3(0, -1, -1)
		},
		{
			new Point3(-1, 0, 0),
			new Point3(-1, 1, 0),
			new Point3(-1, -1, 0)
		},
		{
			new Point3(0, 0, 1),
			new Point3(0, 1, 1),
			new Point3(0, -1, 1)
		},
		{
			new Point3(1, 0, 0),
			new Point3(1, 1, 0),
			new Point3(1, -1, 0)
		}
	};

	public override int[] HandledBlocks => new int[1] { BlocksManager.GetBlockIndex<RailBlock>() };

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		UpdateCornerState(value, x, y, z, 1);
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y - 1, z);
		if (BlocksManager.Blocks[Terrain.ExtractContents(cellValue)].IsTransparent_(cellValue))
		{
			SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
		}
	}

	private void UpdateCornerState(int value, int x, int y, int z, int step)
	{
		int? num = null;
		bool[] array = new bool[4];
		bool[] array2 = new bool[4];
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Point3 point = m_pointsToUpdate[i, j];
				int cellValueFastChunkExists = SubsystemTerrain.Terrain.GetCellValueFastChunkExists(x + point.X, y + point.Y, z + point.Z);
				if (Terrain.ExtractContents(cellValueFastChunkExists) == RailBlock.Index)
				{
					if (j == 1)
					{
						array2[i] = RailBlock.CanConnectTo(RailBlock.GetRailType(Terrain.ExtractData(cellValueFastChunkExists)), i);
					}
					else
					{
						array[i] = RailBlock.CanConnectTo(RailBlock.GetRailType(Terrain.ExtractData(cellValueFastChunkExists)), i);
					}
					if (step > 0)
					{
						UpdateCornerState(cellValueFastChunkExists, x + point.X, y + point.Y, z + point.Z, step - 1);
					}
					break;
				}
			}
		}
		if (array[0] && array[3])
		{
			num = 1;
		}
		else if (array[0] && array[1])
		{
			num = 2;
		}
		else if (array[1] && array[2])
		{
			num = 3;
		}
		else if (array[2] && array[3])
		{
			num = 0;
		}
		else if (array2[2])
		{
			num = 6;
		}
		else if (array2[0])
		{
			num = 8;
		}
		else if (array2[3])
		{
			num = 7;
		}
		else if (array2[1])
		{
			num = 9;
		}
		else if (array[0] || array[2])
		{
			num = 4;
		}
		else if (array[1] || array[3])
		{
			num = 5;
		}
		if (num.HasValue)
		{
			SubsystemTerrain.ChangeCell(x, y, z, Terrain.MakeBlockValue(RailBlock.Index, 0, RailBlock.SetRailType(0, num.Value)));
		}
	}
}
