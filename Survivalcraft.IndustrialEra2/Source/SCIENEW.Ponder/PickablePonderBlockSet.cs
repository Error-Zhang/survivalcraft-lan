using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;

namespace SCIENEW.Ponder;

public class PickablePonderBlockSet : IPonderBlockSet<PickablePonderBlock>, IPonderSet
{
	protected List<PickablePonderBlock> m_pickables = new List<PickablePonderBlock>();

	protected List<PickablePonderBlock> m_pickablesToRemove = new List<PickablePonderBlock>();

	protected Ponder m_ponder;

	protected StaticPonderBlockSet m_staticPonderBlockSet;

	protected DrawBlockEnvironmentData m_drawBlockEnvironmentData = new DrawBlockEnvironmentData();

	protected PrimitivesRenderer3D m_primitiveRenderer = new PrimitivesRenderer3D();

	protected SubsystemTime m_subsystemTime;

	protected SubsystemGameInfo m_subsystemGameInfo;

	public List<PickablePonderBlock> PonderBlocks => m_pickables;

	public DrawBlockEnvironmentData DrawBlockEnvironmentData => m_drawBlockEnvironmentData;

	public PrimitivesRenderer3D PrimitiveRenderer => m_primitiveRenderer;

	public StaticPonderBlockSet StaticPonderBlockSet => m_staticPonderBlockSet ?? (m_staticPonderBlockSet = m_ponder.StaticPonderBlockSet);

	public PickablePonderBlockSet(Project project, Ponder ponder)
	{
		m_ponder = ponder;
		m_subsystemTime = project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemGameInfo = project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
	}

	public PickablePonderBlock Add(int value, Vector3 position, Vector3 velocity, Matrix? stuckMatrix = null, object tag = null)
	{
		Pickable pickable = new Pickable();
		pickable.Value = value;
		pickable.Position = position;
		pickable.Velocity = velocity;
		pickable.StuckMatrix = stuckMatrix;
		pickable.InitializeData(() => StaticPonderBlockSet.m_terrain, () => m_drawBlockEnvironmentData, () => float.MaxValue, () => (Vector3 _, Vector3 _) => 0f, () => m_primitiveRenderer);
		PickablePonderBlock pickablePonderBlock = new PickablePonderBlock(pickable, tag);
		m_pickables.Add(pickablePonderBlock);
		return pickablePonderBlock;
	}

	public void Remove(PickablePonderBlock pickablePonderBlock)
	{
		pickablePonderBlock.Pickable.ToRemove = true;
	}

	public void Draw(Camera camera)
	{
		double totalElapsedGameTime = m_subsystemGameInfo.TotalElapsedGameTime;
		Matrix rotationMatrix = Matrix.CreateRotationY((float)MathUtils.Remainder(totalElapsedGameTime, 6.2831854820251465));
		foreach (PickablePonderBlock pickable in m_pickables)
		{
			try
			{
				pickable.Pickable.Draw(camera, 0, totalElapsedGameTime, rotationMatrix);
			}
			catch (Exception ex)
			{
				if (pickable.Pickable.LogDrawError)
				{
					Log.Error("Pickable draw error: " + ex);
					pickable.Pickable.LogDrawError = false;
				}
			}
		}
		m_primitiveRenderer.Flush(camera.ViewProjectionMatrix);
	}

	public void Update()
	{
		for (int i = 0; i < m_pickables.Count; i++)
		{
			if (m_pickables[i].Paused)
			{
				continue;
			}
			Pickable pickable = m_pickables[i].Pickable;
			lock (pickable)
			{
				if (pickable.ToRemove)
				{
					m_pickablesToRemove.Add(m_pickables[i]);
					continue;
				}
				try
				{
					pickable.Update(m_subsystemTime.GameTimeDelta);
				}
				catch (Exception ex)
				{
					Log.Error("Pickable update error: " + ex);
					pickable.ToRemove = true;
				}
			}
		}
		foreach (PickablePonderBlock item in m_pickablesToRemove)
		{
			lock (m_pickables)
			{
				m_pickables.Remove(item);
			}
		}
		m_pickablesToRemove.Clear();
	}

	public static TerrainRaycastResult? Raycast(Terrain terrain, Vector3 start, Vector3 end, bool useInteractionBoxes, bool skipAirBlocks, Func<int, float, bool> action)
	{
		float num = start.Distance(end);
		if (num > 1000f)
		{
			end = start + 1000f * (end - start).Normalize();
		}
		Ray3 ray = new Ray3(start, Vector3.Normalize(end - start));
		float x = start.X;
		float y = start.Y;
		float z = start.Z;
		float x2 = end.X;
		float y2 = end.Y;
		float z2 = end.Z;
		int num2 = Terrain.ToCell(x);
		int num3 = Terrain.ToCell(y);
		int num4 = Terrain.ToCell(z);
		int num5 = Terrain.ToCell(x2);
		int num6 = Terrain.ToCell(y2);
		int num7 = Terrain.ToCell(z2);
		int num8 = ((x < x2) ? 1 : ((x > x2) ? (-1) : 0));
		int num9 = ((y < y2) ? 1 : ((y > y2) ? (-1) : 0));
		int num10 = ((z < z2) ? 1 : ((z > z2) ? (-1) : 0));
		float num11 = MathF.Floor(x);
		float num12 = num11 + 1f;
		float num13 = ((x > x2) ? (x - num11) : (num12 - x)) / Math.Abs(x2 - x);
		float num14 = MathF.Floor(y);
		float num15 = num14 + 1f;
		float num16 = ((y > y2) ? (y - num14) : (num15 - y)) / Math.Abs(y2 - y);
		float num17 = MathF.Floor(z);
		float num18 = num17 + 1f;
		float num19 = ((z > z2) ? (z - num17) : (num18 - z)) / Math.Abs(z2 - z);
		float num20 = 1f / Math.Abs(x2 - x);
		float num21 = 1f / Math.Abs(y2 - y);
		float num22 = 1f / Math.Abs(z2 - z);
		while (true)
		{
			BoundingBox boundingBox = default;
			int collisionBoxIndex = 0;
			float? num23 = null;
			int cellValue = terrain.GetCellValue(num2, num3, num4);
			int num24 = Terrain.ExtractContents(cellValue);
			if (num24 != 0 || !skipAirBlocks)
			{
				Ray3 ray2 = new Ray3(ray.Position - new Vector3(num2, num3, num4), ray.Direction);
				float? num25 = BlocksManager.Blocks[num24].Raycast(ray2, null, cellValue, useInteractionBoxes, out var nearestBoxIndex, out var nearestBox);
				if (num25.HasValue && (!num23.HasValue || num25.Value < num23.Value))
				{
					num23 = num25;
					collisionBoxIndex = nearestBoxIndex;
					boundingBox = nearestBox;
				}
			}
			if (num23.HasValue && num23.Value <= num && (action == null || action(cellValue, num23.Value)))
			{
				int face = 0;
				Vector3 vector = start - new Vector3(num2, num3, num4) + num23.Value * ray.Direction;
				float num26 = float.MaxValue;
				float num27 = MathF.Abs(vector.X - boundingBox.Min.X);
				if (num27 < num26)
				{
					num26 = num27;
					face = 3;
				}
				num27 = MathF.Abs(vector.X - boundingBox.Max.X);
				if (num27 < num26)
				{
					num26 = num27;
					face = 1;
				}
				num27 = MathF.Abs(vector.Y - boundingBox.Min.Y);
				if (num27 < num26)
				{
					num26 = num27;
					face = 5;
				}
				num27 = MathF.Abs(vector.Y - boundingBox.Max.Y);
				if (num27 < num26)
				{
					num26 = num27;
					face = 4;
				}
				num27 = MathF.Abs(vector.Z - boundingBox.Min.Z);
				if (num27 < num26)
				{
					num26 = num27;
					face = 2;
				}
				num27 = MathF.Abs(vector.Z - boundingBox.Max.Z);
				if (num27 < num26)
				{
					num26 = num27;
					face = 0;
				}
				return new TerrainRaycastResult
				{
					Ray = ray,
					Value = cellValue,
					CellFace = new CellFace
					{
						X = num2,
						Y = num3,
						Z = num4,
						Face = face
					},
					CollisionBoxIndex = collisionBoxIndex,
					Distance = num23.Value
				};
			}
			if (num13 <= num16 && num13 <= num19)
			{
				if (num2 == num5)
				{
					break;
				}
				num13 += num20;
				num2 += num8;
			}
			else if (num16 <= num13 && num16 <= num19)
			{
				if (num3 == num6)
				{
					break;
				}
				num16 += num21;
				num3 += num9;
			}
			else
			{
				if (num4 == num7)
				{
					break;
				}
				num19 += num22;
				num4 += num10;
			}
		}
		return null;
	}
}
