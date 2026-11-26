using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;

namespace SCIENEW;

public class HearthFurnaceCastingBasinDeviceBehavior : EntityDeviceBehavior<ComponentHearthFurnaceCastingBasin>
{
	public SubsystemPickables m_subsystemPickable;

	public new SubsystemAudio m_subsystemAudio;

	public HearthFurnaceCastingBasinDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemPickable = project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_subsystemAudio = project.FindSubsystem<SubsystemAudio>(throwOnError: true);
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z);
		if (blockEntity == null)
		{
			componentMiner.ComponentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get("Behaviors", "HearthFurnaceCastingBasinDeviceBehavior", "CastingBasinBrokenTip"), Color.White, blinking: false, playNotificationSound: false);
			return false;
		}
		ComponentHearthFurnaceCastingBasin componentHearthFurnaceCastingBasin = blockEntity.Entity.FindComponent<ComponentHearthFurnaceCastingBasin>();
		if (componentHearthFurnaceCastingBasin == null)
		{
			componentMiner.ComponentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get("Behaviors", "HearthFurnaceCastingBasinDeviceBehavior", "CastingBasinBrokenTip"), Color.White, blinking: false, playNotificationSound: false);
			return false;
		}
		Vector3 vector = componentMiner.ComponentPlayer.ComponentBody.Position - (new Vector3(raycastResult.CellFace.Point) + new Vector3(0.5f));
		vector = new Vector3(vector.X, 0f, vector.Z);
		vector = vector.Normalize();
		if (componentHearthFurnaceCastingBasin.GetSlotCount(componentHearthFurnaceCastingBasin.ResultSlotIndex) > 0)
		{
			m_subsystemPickable.AddPickable(componentHearthFurnaceCastingBasin.GetSlotValue(componentHearthFurnaceCastingBasin.ResultSlotIndex), 1, new Vector3(raycastResult.CellFace.Point) + new Vector3(0.5f, 0.75f, 0.5f), Vector3.UnitY * 2f + vector * 3f, null);
			componentHearthFurnaceCastingBasin.RemoveSlotItems(componentHearthFurnaceCastingBasin.ResultSlotIndex, 1);
			m_subsystemAudio.PlaySound("Audio/BlockPlaced", 1f, 0.3f, new Vector3(raycastResult.CellFace.Point), 2.5f, autoDelay: true);
			return true;
		}
		int activeBlockValue = componentMiner.ActiveBlockValue;
		int num = Terrain.ExtractContents(activeBlockValue);
		if (componentHearthFurnaceCastingBasin.GetSlotCount(componentHearthFurnaceCastingBasin.ModuleSlotIndex) > 0)
		{
			m_subsystemPickable.AddPickable(componentHearthFurnaceCastingBasin.GetSlotValue(componentHearthFurnaceCastingBasin.ModuleSlotIndex), 1, new Vector3(raycastResult.CellFace.Point) + new Vector3(0.5f, 0.75f, 0.5f), Vector3.UnitY * 2f + vector * 3f, null);
			componentHearthFurnaceCastingBasin.RemoveSlotItems(componentHearthFurnaceCastingBasin.ModuleSlotIndex, 1);
		}
		if (num == BaseItemBlock.Index && ItemsBlockManager.GetItem(activeBlockValue) is ModuleItem)
		{
			IInventory inventory = componentMiner.Inventory;
			int activeSlotIndex = componentMiner.Inventory.ActiveSlotIndex;
			if (ComponentInventoryBase.AcquireItems(componentHearthFurnaceCastingBasin, activeBlockValue, 1) == 0)
			{
				inventory.RemoveSlotItems(activeSlotIndex, 1);
				m_subsystemAudio.PlaySound("Audio/BlockPlaced", 1f, 0.3f, new Vector3(raycastResult.CellFace.Point), 2.5f, autoDelay: true);
				return true;
			}
		}
		return true;
	}
}
