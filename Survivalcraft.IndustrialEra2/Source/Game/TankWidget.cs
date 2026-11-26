using System;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using SCIENEW;
using SCIENEW.Extensions;

namespace Game;

public class TankWidget : CanvasWidget, IDragTargetWidget
{
	public BevelledRectangleWidget m_rectangleWidget;

	public FluidWidget m_fluidWidget;

	public ClickableWidget m_clickableWidget;

	public ITank m_tank;

	public int m_tankIndex;

	public GameWidget m_gameWidget;

	public DragHostWidget m_dragHostWidget;

	public InventoryDragData m_inventoryDragData;

	public CustomFontLabelWidget m_volumeLabel;

	private bool IsShowName => m_clickableWidget.IsChecked;

	public float BevelSize
	{
		get
		{
			return m_rectangleWidget.BevelSize;
		}
		set
		{
			m_rectangleWidget.BevelSize = value;
		}
	}

	public GameWidget GameWidget
	{
		get
		{
			if (m_gameWidget == null)
			{
				for (ContainerWidget parentWidget = ParentWidget; parentWidget != null; parentWidget = parentWidget.ParentWidget)
				{
					if (parentWidget is GameWidget gameWidget)
					{
						m_gameWidget = gameWidget;
						break;
					}
				}
			}
			return m_gameWidget;
		}
	}

	public DragHostWidget DragHostWidget
	{
		get
		{
			if (m_dragHostWidget == null)
			{
				m_dragHostWidget = ((GameWidget != null) ? GameWidget.Children.Find<DragHostWidget>(throwIfNotFound: false) : null);
			}
			return m_dragHostWidget;
		}
	}

	public TankWidget()
	{
		Size = new Vector2(72f, 72f);
		m_rectangleWidget = new BevelledRectangleWidget
		{
			BevelSize = -2f,
			DirectionalLight = 0.15f,
			CenterColor = Color.Black
		};
		Children.Add(m_rectangleWidget);
		m_fluidWidget = new FluidWidget
		{
			Size = Size - new Vector2(MathF.Abs(BevelSize), MathF.Abs(BevelSize)) * 2f,
			Margin = new Vector2(MathF.Abs(BevelSize), MathF.Abs(BevelSize))
		};
		Children.Add(m_fluidWidget);
		m_clickableWidget = new ClickableWidget
		{
			Margin = new Vector2(MathF.Abs(BevelSize), MathF.Abs(BevelSize)),
			HorizontalAlignment = WidgetAlignment.Center,
			VerticalAlignment = WidgetAlignment.Center,
			IsAutoCheckingEnabled = true
		};
		Children.Add(m_clickableWidget);
		CanvasWidget canvasWidget = new CanvasWidget
		{
			Size = new Vector2(float.MaxValue, 20f),
			HorizontalAlignment = WidgetAlignment.Stretch,
			VerticalAlignment = WidgetAlignment.Near
		};
		Children.Add(canvasWidget);
		m_volumeLabel = new CustomFontLabelWidget
		{
			FontScale = 0.45f,
			TextOffset = new Vector2(-6f, 0f),
			Color = Color.Green,
			Font = IndustrialModLoader.PixelFont,
			HorizontalAlignment = WidgetAlignment.Center,
			VerticalAlignment = WidgetAlignment.Center,
			Text = "0/4L"
		};
		canvasWidget.Children.Add(m_volumeLabel);
	}

	public void AssignTank(ITank tank, int tankIndex)
	{
		m_tank = tank;
		m_tankIndex = tankIndex;
	}

	public override void Update()
	{
		base.Update();
		if (m_tank != null && m_tankIndex >= 0 && m_tankIndex < m_tank.TanksCount)
		{
			if (m_clickableWidget.IsClicked)
			{
				AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
			}
			m_fluidWidget.m_fluidValue = m_tank.GetTankValue(m_tankIndex);
			m_fluidWidget.m_relativeVolume = m_tank.GetTankVolume(m_tankIndex) / m_tank.GetTankOriginalCapacity(m_tankIndex);
			m_fluidWidget.Size = Size - new Vector2(MathF.Abs(BevelSize), MathF.Abs(BevelSize)) * 2f;
			m_volumeLabel.Text = (IsShowName ? FluidsManager.GetFluid(m_tank.GetTankValue(m_tankIndex)).GetDisplayName(m_tank.GetTankValue(m_tankIndex)) : (m_tank.GetTankVolume(m_tankIndex).ToString("F1") + "/" + m_tank.GetTankOriginalCapacity(m_tankIndex).ToString("F1") + "L"));
		}
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		IsDrawRequired = m_inventoryDragData != null;
		base.MeasureOverride(parentAvailableSize);
	}

	public override void Draw(DrawContext dc)
	{
		if (m_tank != null && m_inventoryDragData != null && m_tank.GetTankOriginalCapacity(m_tankIndex) > 0f)
		{
			GlobalTransform.Right.Length();
			(ActualSize / 2f).Transform(GlobalTransform);
			Vector2 vector = (Vector2.Zero - Vector2.One * 24f).Transform(GlobalTransform);
			Vector2 vector2 = (ActualSize + Vector2.One * 24f).Transform(GlobalTransform);
			FlatBatch2D flatBatch2D = dc.PrimitivesRenderer2D.FlatBatch(100);
			flatBatch2D.QueueRectangle(vector, vector2, 0f, new Color(0, 0, 0, 96) * GlobalColorTransform);
			flatBatch2D.QueueRectangle(vector + Vector2.One * 0.5f, vector2 - Vector2.One * 0.5f, 0f, new Color(0, 0, 0, 64) * GlobalColorTransform);
			flatBatch2D.QueueRectangle(vector - Vector2.One * 0.5f, vector2 + Vector2.One * 0.5f, 0f, new Color(0, 0, 0, 48) * GlobalColorTransform);
			flatBatch2D.QueueQuad(vector, vector2, 0f, new Color(0, 0, 0, 48) * GlobalColorTransform);
		}
		m_inventoryDragData = null;
	}

	public void DragOver(Widget dragWidget, object data)
	{
		m_inventoryDragData = data as InventoryDragData;
	}

	public void DragDrop(Widget dragWidget, object data)
	{
		InventoryDragData inventoryDragData = data as InventoryDragData;
		if (m_tank != null && inventoryDragData != null)
		{
			HandleDragDrop(inventoryDragData.Inventory, inventoryDragData.SlotIndex, inventoryDragData.DragMode, m_tank, m_tankIndex);
		}
	}

	public bool HandleDragDrop(IInventory sourceInventory, int sourceSlotIndex, DragMode dragMode, ITank targetTank, int targetTankIndex)
	{
		if (IndustrialEraLanAdapter.IsRemote(targetTank.Project))
		{
			ComponentPlayer player = (sourceInventory as Component)?.Entity.FindComponent<ComponentPlayer>();
			return player?.ComponentMiner.Inventory == sourceInventory
				&& IndustrialEraLanAdapter.QueueTankTransfer(
					player, sourceSlotIndex, dragMode, targetTank, targetTankIndex, sourceInventory.GetSlotValue(sourceSlotIndex));
		}
		return TransferFluid(sourceInventory, sourceSlotIndex, dragMode == DragMode.SingleItem, targetTank, targetTankIndex);
	}

	internal static bool TransferFluid(IInventory sourceInventory, int sourceSlotIndex, bool singleItem, ITank targetTank, int targetTankIndex)
	{
		bool flag = false;
		int slotValue = sourceInventory.GetSlotValue(sourceSlotIndex);
		int tankValue = targetTank.GetTankValue(targetTankIndex);
		int num = sourceInventory.GetSlotCount(sourceSlotIndex);
		float tankVolume = targetTank.GetTankVolume(targetTankIndex);
		if (singleItem)
		{
			num = MathUtils.Min(num, 1);
		}
		ComponentPlayer componentPlayer = ((sourceInventory is Component component) ? component.Entity.FindComponent<ComponentPlayer>() : null);
		Fluid[] fluids = FluidsManager.Fluids;
		foreach (Fluid fluid in fluids)
		{
			if (!(fluid is IScoopableFluid scoopableFluid))
			{
				continue;
			}
			int fluidID = FluidsManager.GetFluidID(fluid);
			if (scoopableFluid.AcquireContainer(fluidID).ContainsValue(slotValue))
			{
				if (tankVolume > 0f && tankValue != fluidID)
				{
					break;
				}
				float num2 = targetTank.GetTankCapacity(targetTankIndex, fluidID) - tankVolume;
				if (num2 <= 0f)
				{
					break;
				}
				float containerVolume = FluidsManager.GetContainerVolume(slotValue);
				int num3 = sourceInventory.RemoveSlotItems(sourceSlotIndex, num);
				num = Math.Min(num3, (int)MathF.Floor(num2 / containerVolume));
				float volume = containerVolume * (float)num;
				int value = scoopableFluid.AcquireContainer(fluidID).GetBySecond(slotValue);
				targetTank.AddTankFluid(targetTankIndex, fluidID, volume, out var overflow);
				if (overflow > 0.5f)
				{
					AudioManager.PlaySound("Audio/Splashes/Splash1", 1f, 0f, 0f);
				}
				if (num3 > num)
				{
					sourceInventory.AddSlotItems(sourceSlotIndex, slotValue, num3 - num);
				}
				int num4 = ComponentInventoryBase.AcquireItems(sourceInventory, value, num);
				if (num4 > 0)
				{
					componentPlayer?.Project.FindSubsystem<SubsystemPickables>().AddPickable(value, num4, componentPlayer.ComponentBody.Position, Vector3.Zero, null);
				}
				flag = true;
			}
		}
		if (!flag)
		{
			Fluid fluid2 = FluidsManager.GetFluid(tankValue);
			if (fluid2 is IScoopableFluid scoopableFluid2)
			{
				int fluidID2 = FluidsManager.GetFluidID(fluid2);
				float containerVolume2 = FluidsManager.GetContainerVolume(slotValue);
				if (scoopableFluid2.AcquireContainer(fluidID2).ContainsKey(slotValue))
				{
					int num5 = sourceInventory.RemoveSlotItems(sourceSlotIndex, num);
					num = Math.Min(num5, (int)Math.Floor(tankVolume / containerVolume2));
					if (num > 0)
					{
						float volume2 = FluidsManager.GetContainerVolume(slotValue) * (float)num;
						int value2 = scoopableFluid2.AcquireContainer(fluidID2).GetByFirst(slotValue);
						targetTank.RemoveTankFluid(targetTankIndex, volume2);
						if (num5 > num)
						{
							sourceInventory.AddSlotItems(sourceSlotIndex, slotValue, num5 - num);
						}
						int num6 = ComponentInventoryBase.AcquireItems(sourceInventory, value2, num);
						if (num6 > 0)
						{
							componentPlayer?.Project.FindSubsystem<SubsystemPickables>().AddPickable(value2, num6, componentPlayer.ComponentBody.Position, Vector3.Zero, null);
						}
						flag = true;
					}
					else if (num5 > 0)
					{
						sourceInventory.AddSlotItems(sourceSlotIndex, slotValue, num5);
					}
				}
			}
		}
		if (flag)
		{
			AudioManager.PlaySound("Audio/UI/ItemMoved", 1f, 0f, 0f);
		}
		return flag;
	}
}
