using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SifterDeviceBehavior : EntityDeviceBehavior<ComponentSifter>
{
	public new SubsystemBlockEntities m_subsystemBlockEntities;

	public SifterDeviceBehavior(Project project)
		: base(project)
	{
		m_subsystemBlockEntities = project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true);
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z);
		if (blockEntity == null)
		{
			componentMiner.ComponentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get("Behaviors", "SifterDeviceBehavior", "BrokenTip"), Color.White, blinking: false, playNotificationSound: false);
			return false;
		}
		ComponentSifter componentSifter = blockEntity.Entity.FindComponent<ComponentSifter>();
		if (componentSifter == null)
		{
			componentMiner.ComponentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get("Behaviors", "SifterDeviceBehavior", "BrokenTip"), Color.White, blinking: false, playNotificationSound: false);
			return false;
		}
		IInventory inventory = componentMiner.Inventory;
		int activeSlotIndex = componentMiner.Inventory.ActiveSlotIndex;
		int activeBlockValue = componentMiner.ActiveBlockValue;
		if (ComponentInventoryBase.AcquireItems(componentSifter, activeBlockValue, 1) == 0)
		{
			inventory.RemoveSlotItems(activeSlotIndex, 1);
			return true;
		}
		return componentSifter.Sift();
	}
}
