using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions.GameLogic;
using SCIENEW.Extensions.Graphics;
using SCIENEW.ProductionIO;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentGReactorTop : ComponentInventoryBase, IDrawable, IUpdateable, IInventoryProductionSlots
{
	private static readonly int[] m_rodSlotIndices = new int[16]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11, 12, 13, 14, 15
	};

	private static readonly int[] m_emptySlotIndices = Array.Empty<int>();

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemEnginePower m_subsystemEnginePower;

	public SubsystemPickables m_subsystemPickables;

	public SubsystemProjectiles m_subsystemProjectiles;

	public SubsystemTime m_subsystemTime;

	public SubsystemAudio m_subsystemAudio;

	private PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

	private DrawBlockEnvironmentData m_drawBlockEnvironmentData = new DrawBlockEnvironmentData();

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int[] DrawOrders => new int[1];

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (Terrain.ExtractContents(value) != BlocksManager.GetBlockIndex<BaseDamageableItemBlock>() || !(DamageableItemManager.GetItem(value) is FuelRodDItem))
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemEnginePower = Project.FindSubsystem<SubsystemEnginePower>(throwOnError: true);
		m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_subsystemProjectiles = Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
	}

	public void Draw(Camera camera, int drawOrder)
	{
		if (!m_subsystemTerrain.IsVisible(camera, new Vector3(m_componentBlockEntity.Coordinates)) || !camera.ViewFrustum.Intersection(new BoundingSphere(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f), 1f)))
		{
			return;
		}
		Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.1875f) + new Vector3(0f, 0.65f, 0f);
		_ = new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(25f / 32f) + new Vector3(0f, 0.65f, 0f);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				int slotIndex = i * 4 + j;
				if (GetSlotCount(slotIndex) != 0)
				{
					int slotValue = GetSlotValue(slotIndex);
					if (Terrain.ExtractContents(slotValue) == BlocksManager.GetBlockIndex<BaseDamageableItemBlock>() && DamageableItemManager.GetItem(slotValue) is FuelRodDItem fuelRodDItem)
					{
						int num = 2 + i * 5;
						int num2 = 2 + j * 5;
						Matrix matrix = Matrix.CreateTranslation(new Vector3(vector.X + (float)num / 32f, vector.Y, vector.Z + (float)num2 / 32f));
						fuelRodDItem.DrawBlock(m_primitivesRenderer, slotValue, Color.White, 0.6f, ref matrix, m_drawBlockEnvironmentData);
					}
				}
			}
		}
		m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
	}

	public void Update(float dt)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		m_drawBlockEnvironmentData.ModifyEnvironmentData(m_subsystemTerrain, coordinates);
	}

	public IEnumerable<int> GetSlotIndices(ProductionSlotKind kind)
	{
		if (kind == ProductionSlotKind.Input)
		{
			return m_rodSlotIndices;
		}
		return m_emptySlotIndices;
	}
}
