using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public abstract class SubsystemFocalorsFluidBlockBehavior : SubsystemBlockBehavior
{
	public class SpreadData
	{
		public float Volume;

		public bool Destroy;

		public SpreadData(float volume, bool destroy)
		{
			Volume = volume;
			Destroy = destroy;
		}
	}

	public static readonly Point2[] m_sideNeighbors = new Point2[4]
	{
		new Point2(-1, 0),
		new Point2(1, 0),
		new Point2(0, -1),
		new Point2(0, 1)
	};

	public SubsystemTime SubsystemTime;

	public readonly int m_handleBlock;

	public readonly HashSet<Point3> m_toUpdate = new HashSet<Point3>();

	public readonly Dictionary<Point3, SpreadData> m_toSpread = new Dictionary<Point3, SpreadData>();

	public readonly Dictionary<Point3, float> m_toDraw = new Dictionary<Point3, float>();

	public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

	public FontBatch3D m_fontBatch3D;

	public int[] DrawOrders => new int[1] { 1000 };

	public SubsystemFocalorsFluidBlockBehavior(int handleBlock)
	{
		m_handleBlock = handleBlock;
	}

	public override void Load(ValuesDictionary valuesDictionary)
	{
		base.Load(valuesDictionary);
		SubsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_fontBatch3D = m_primitivesRenderer3D.FontBatch(LabelWidget.BitmapFont, 1, DepthStencilState.DepthRead, RasterizerState.CullNoneScissor, BlendState.AlphaBlend, SamplerState.LinearClamp);
	}

	public void SpreadFluid()
	{
		Terrain terrain = SubsystemTerrain.Terrain;
		Point3 key;
		foreach (Point3 item in m_toUpdate)
		{
			int cellValue = terrain.GetCellValue(item.X, item.Y, item.Z);
			if (Terrain.ExtractContents(cellValue) != m_handleBlock)
			{
				m_toSpread.Remove(item);
				continue;
			}
			bool flag = false;
			float num;
			if (m_toSpread.TryGetValue(item, out SpreadData value))
			{
				num = value.Volume;
				flag = true;
			}
			else
			{
				num = 1f - FocalorsFluidBlock.GetLevel(Terrain.ExtractData(cellValue));
			}
			if (num < 0.01f)
			{
				continue;
			}
			float num2 = num;
			if (item.Y > 0)
			{
				key = new Point3
				{
					X = item.X,
					Y = item.Y - 1,
					Z = item.Z
				};
				Point3 key2 = key;
				int cellValueFast = terrain.GetCellValueFast(item.X, item.Y - 1, item.Z);
				int num3 = Terrain.ExtractContents(cellValueFast);
				if (m_toSpread.TryGetValue(key2, out SpreadData value2))
				{
					float num4 = 1f - value2.Volume;
					if (num4 >= num)
					{
						if (flag)
						{
							value.Volume = 0f;
						}
						else
						{
							m_toSpread.TryAdd(item, new SpreadData(0f, destroy: false));
						}
						value2.Volume += num;
						continue;
					}
					num -= num4;
					value2.Volume = 1f;
				}
				else
				{
					if (num3 == AirBlock.Index)
					{
						if (flag)
						{
							value.Volume = 0f;
						}
						else
						{
							m_toSpread.TryAdd(item, new SpreadData(0f, destroy: false));
						}
						m_toSpread.TryAdd(key2, new SpreadData(num, destroy: false));
						continue;
					}
					if (num3 == m_handleBlock)
					{
						float level = FocalorsFluidBlock.GetLevel(Terrain.ExtractData(cellValueFast));
						if (level >= num)
						{
							if (flag)
							{
								value.Volume = 0f;
							}
							else
							{
								m_toSpread.TryAdd(item, new SpreadData(0f, destroy: false));
							}
							m_toSpread.TryAdd(key2, new SpreadData(1f - level + num, destroy: false));
							continue;
						}
						if (level > 0f)
						{
							m_toSpread.TryAdd(key2, new SpreadData(1f, destroy: false));
							num -= level;
						}
					}
					else if (!BlocksManager.Blocks[num3].IsFluidBlocker_(cellValueFast))
					{
						if (flag)
						{
							value.Volume = 0f;
						}
						else
						{
							m_toSpread.TryAdd(item, new SpreadData(0f, destroy: false));
						}
						m_toSpread.TryAdd(key2, new SpreadData(num, destroy: true));
						continue;
					}
				}
			}
			float value4;
			if (num > 0.01f)
			{
				Dictionary<Point3, float> dictionary = new Dictionary<Point3, float>();
				HashSet<Point3> hashSet = new HashSet<Point3>();
				Point2[] sideNeighbors = m_sideNeighbors;
				for (int i = 0; i < sideNeighbors.Length; i++)
				{
					Point2 point = sideNeighbors[i];
					key = new Point3
					{
						X = item.X + point.X,
						Y = item.Y - 1,
						Z = item.Z + point.Y
					};
					Point3 point2 = key;
					if (m_toSpread.TryGetValue(point2, out SpreadData value3))
					{
						if (value3.Volume < 1f)
						{
							dictionary.Add(point2, value3.Volume);
						}
						continue;
					}
					int cellValue2 = terrain.GetCellValue(point2.X, point2.Y, point2.Z);
					int num5 = Terrain.ExtractContents(cellValue2);
					if (num5 == AirBlock.Index)
					{
						dictionary.Add(point2, 0f);
					}
					else if (num5 == m_handleBlock)
					{
						float num6 = 1f - FocalorsFluidBlock.GetLevel(Terrain.ExtractData(cellValue2));
						if (num6 < 1f)
						{
							dictionary.Add(point2, num6);
						}
					}
					else if (!BlocksManager.Blocks[num5].IsFluidBlocker_(cellValue2))
					{
						dictionary.Add(point2, 0f);
						hashSet.Add(point2);
					}
				}
				if (dictionary.Count > 0)
				{
					while (dictionary.Count > 0)
					{
						float num7 = dictionary.Max((KeyValuePair<Point3, float> pair) => pair.Value);
						float num8 = 1f - num7;
						float num9 = num8 * (float)dictionary.Count;
						if (num >= num9)
						{
							HashSet<Point3> hashSet2 = new HashSet<Point3>();
							foreach (KeyValuePair<Point3, float> item2 in dictionary)
							{
								item2.Deconstruct(out key, out value4);
								Point3 point3 = key;
								float num10 = value4;
								if (m_toSpread.TryGetValue(point3, out SpreadData value5))
								{
									value5.Volume += num8;
									if (value5.Volume >= 0.99f)
									{
										hashSet2.Add(point3);
									}
								}
								else
								{
									m_toSpread.TryAdd(point3, new SpreadData(num10 + num8, destroy: false));
								}
							}
							foreach (Point3 item3 in hashSet2)
							{
								dictionary.Remove(item3);
							}
							num -= num9;
							continue;
						}
						float num11 = num / (float)dictionary.Count;
						foreach (KeyValuePair<Point3, float> item4 in dictionary)
						{
							item4.Deconstruct(out key, out value4);
							Point3 point4 = key;
							float num12 = value4;
							if (m_toSpread.TryGetValue(point4, out SpreadData value6))
							{
								value6.Volume += num11;
							}
							else if (hashSet.Contains(point4))
							{
								m_toSpread.TryAdd(point4, new SpreadData(num11, destroy: true));
							}
							else
							{
								m_toSpread.TryAdd(point4, new SpreadData(num12 + num11, destroy: false));
							}
						}
						dictionary.Clear();
						num = 0f;
					}
				}
			}
			if (num > 0.01f)
			{
				Dictionary<Point3, float> dictionary2 = new Dictionary<Point3, float>();
				HashSet<Point3> hashSet3 = new HashSet<Point3>();
				Point2[] sideNeighbors = m_sideNeighbors;
				for (int i = 0; i < sideNeighbors.Length; i++)
				{
					Point2 point5 = sideNeighbors[i];
					key = new Point3
					{
						X = item.X + point5.X,
						Y = item.Y,
						Z = item.Z + point5.Y
					};
					Point3 point6 = key;
					if (m_toSpread.TryGetValue(point6, out SpreadData value7))
					{
						if (value7.Volume < num)
						{
							dictionary2.Add(point6, value7.Volume);
						}
						continue;
					}
					int cellValue3 = terrain.GetCellValue(point6.X, point6.Y, point6.Z);
					int num13 = Terrain.ExtractContents(cellValue3);
					if (num13 == AirBlock.Index)
					{
						dictionary2.Add(point6, 0f);
					}
					else if (num13 == m_handleBlock)
					{
						float num14 = 1f - FocalorsFluidBlock.GetLevel(Terrain.ExtractData(cellValue3));
						if (num14 < num)
						{
							dictionary2.Add(point6, num14);
						}
					}
					else if (!BlocksManager.Blocks[num13].IsFluidBlocker_(cellValue3))
					{
						dictionary2.Add(point6, 0f);
						hashSet3.Add(point6);
					}
				}
				if (dictionary2.Count > 0)
				{
					float num15 = (dictionary2.Sum((KeyValuePair<Point3, float> pair) => pair.Value) + num) / (float)(dictionary2.Count + 1);
					if (num15 > 0.005f)
					{
						num = num15;
						foreach (KeyValuePair<Point3, float> item5 in dictionary2)
						{
							item5.Deconstruct(out key, out value4);
							Point3 point7 = key;
							if (Math.Abs(value4 - num15) > 0.005f)
							{
								if (m_toSpread.TryGetValue(point7, out SpreadData value8))
								{
									value8.Volume = num15;
								}
								else if (hashSet3.Contains(point7))
								{
									m_toSpread.TryAdd(point7, new SpreadData(num15, destroy: true));
								}
								else
								{
									m_toSpread.TryAdd(point7, new SpreadData(num15, destroy: false));
								}
							}
						}
					}
					else
					{
						num = 0f;
					}
				}
			}
			if (Math.Abs(num2 - num) > 0.005f)
			{
				if (flag)
				{
					value.Volume = ((num > 0.01f) ? num : 0f);
				}
				else
				{
					m_toSpread.TryAdd(item, new SpreadData((num > 0.01f) ? num : 0f, destroy: false));
				}
			}
		}
		m_toUpdate.Clear();
		foreach (KeyValuePair<Point3, SpreadData> item6 in m_toSpread)
		{
			item6.Deconstruct(out key, out var value9);
			Point3 point8 = key;
			SpreadData spreadData = value9;
			int num16 = ((spreadData.Volume < 0.01f) ? AirBlock.Index : Terrain.MakeBlockValue(m_handleBlock, 0, FocalorsFluidBlock.SetLevel(0, ((double)spreadData.Volume > 0.99) ? 0f : (1f - spreadData.Volume))));
			if (spreadData.Destroy)
			{
				SubsystemTerrain.DestroyCell(0, point8.X, point8.Y, point8.Z, num16, noDrop: false, noParticleSystem: false);
			}
			else
			{
				SubsystemTerrain.ChangeCell(point8.X, point8.Y, point8.Z, num16);
			}
		}
		m_toSpread.Clear();
	}

	public float FluidDropDown(int x, int y, int z, int startValue)
	{
		int value = startValue;
		Terrain terrain = SubsystemTerrain.Terrain;
		while (y < 255)
		{
			int num = ((y != 254) ? terrain.GetCellValueFast(x, y + 1, z) : 0);
			if (y == 254 || Terrain.ExtractContents(num) != m_handleBlock || (m_toSpread.TryGetValue(new Point3(x, y + 1, z), out SpreadData value2) && value2.Volume < 0.01f))
			{
				Point3 key = new Point3
				{
					X = x,
					Y = y,
					Z = z
				};
				float result;
				if (m_toSpread.TryGetValue(key, out SpreadData value3))
				{
					result = value3.Volume;
					value3.Volume = 0f;
				}
				else
				{
					result = 1f - FocalorsFluidBlock.GetLevel(Terrain.ExtractData(value));
					m_toSpread.TryAdd(key, new SpreadData(0f, destroy: false));
				}
				return result;
			}
			value = num;
			y++;
		}
		return 0f;
	}

	public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
	{
		m_toUpdate.Add(new Point3
		{
			X = x,
			Y = y,
			Z = z
		});
		UpdateIsTop(value, x, y, z);
		m_toDraw.TryAdd(new Point3(x, y, z), FocalorsFluidBlock.GetLevel(Terrain.ExtractData(value)));
	}

	public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
	{
		m_toDraw.Remove(new Point3(x, y, z));
	}

	public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
	{
		UpdateIsTop(value, x, y, z);
		m_toDraw[new Point3(x, y, z)] = FocalorsFluidBlock.GetLevel(Terrain.ExtractData(value));
	}

	public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
	{
		m_toUpdate.Add(new Point3
		{
			X = x,
			Y = y,
			Z = z
		});
		if (neighborY == y + 1 || neighborY == y - 1)
		{
			UpdateIsTop(SubsystemTerrain.Terrain.GetCellValueFast(x, y, z), x, y, z);
		}
	}

	public override void OnItemHarvested(int x, int y, int z, int blockValue, ref BlockDropValue dropValue, ref int newBlockValue)
	{
		newBlockValue = m_handleBlock;
		dropValue.Value = 0;
		dropValue.Count = 0;
	}

	public void UpdateIsTop(int value, int x, int y, int z)
	{
		Terrain terrain = SubsystemTerrain.Terrain;
		if (terrain.GetChunkAtCell(x, z) != null)
		{
			int data = Terrain.ExtractData(value);
			bool isTop = y >= 255 || terrain.GetCellContentsFast(x, y + 1, z) != m_handleBlock || FocalorsFluidBlock.GetLevel(data) > 0f;
			int data2 = FocalorsFluidBlock.SetIsTop(data, isTop);
			terrain.SetCellValueFast(x, y, z, Terrain.ReplaceData(value, data2));
		}
	}

	public void Draw(Camera camera, int drawOrder)
	{
		Vector3 viewPosition = camera.ViewPosition;
		foreach (KeyValuePair<Point3, float> item in m_toDraw)
		{
			item.Deconstruct(out var key, out var value);
			Point3 point = key;
			float num = value;
			Vector3 vector = new Vector3((float)point.X + 0.5f, (float)point.Y + 1.5f, (float)point.Z + 0.5f);
			Vector3 v = (vector - viewPosition).Normalize();
			FontBatch3D fontBatch3D = m_fontBatch3D;
			value = 1f - num;
			fontBatch3D.QueueText(value.ToString("0.00"), vector, v.Cross(Vector3.UnitY) * 0.01f, -Vector3.UnitY * 0.01f, Color.White, TextAnchor.HorizontalCenter);
		}
		m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
	}
}
