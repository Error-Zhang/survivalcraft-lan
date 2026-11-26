using System.Linq;
using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Utils;

namespace SCIENEW;

public class ScrewDriversItemBehavior : ItemBehavior
{
	public SubsystemTerrain m_subsystemTerrain;

	public bool m_reverse;

	public ScrewDriversItemBehavior(Project project, bool reverse)
		: base(project)
	{
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_reverse = reverse;
	}

	public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
	{
		TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Digging);
		if (terrainRaycastResult.HasValue)
		{
			if (BlockEntityUtils.GetBlockEntity(m_subsystemTerrain, terrainRaycastResult.Value.CellFace.Point, out ComponentBlockEntity blockEntity))
			{
				IFocus focus = blockEntity.Entity.FindComponent<IFocus>();
				if (focus != null)
				{
					Vector3 vector = new Vector3(terrainRaycastResult.Value.CellFace.Point) + new Vector3(0.5f);
					if (IndustrialEraLanAdapter.IsRemote(Project)) return true;
					Vector3 viewPosition = ray.Position;
					if (m_reverse)
					{
						focus.Focus(2f * vector - viewPosition);
					}
					else
					{
						focus.Focus(viewPosition);
					}
					return true;
				}
			}
			Point3 point = terrainRaycastResult.Value.CellFace.Point;
			IRotatableDevice rotatableDevice = BlockInterfaceResolver.ResolveAt<IRotatableDevice>(m_subsystemTerrain, point, out var blockValue);
			if (rotatableDevice != null)
			{
				if (IndustrialEraLanAdapter.IsRemote(Project)) return true;
				int nextDirection = rotatableDevice.GetNextDirection(blockValue, m_reverse);
				m_subsystemTerrain.ChangeCell(point.X, point.Y, point.Z, nextDirection);
				return true;
			}
		}
		if (ItemsBlockManager.GetItem(componentMiner.ActiveBlockValue) is ScrewDriversItem screwDriversItem)
		{
			bool reverse = screwDriversItem.m_reverse;
			reverse = !reverse;
			ScrewDriversItem item = ItemsBlockManager.GetItems<ScrewDriversItem>().First((ScrewDriversItem x) => x.m_reverse == reverse);
			if (!IndustrialEraLanAdapter.IsRemote(Project))
			{
				componentMiner.RemoveActiveTool(1);
				componentMiner.Inventory.AddSlotItems(componentMiner.Inventory.ActiveSlotIndex, ItemsBlockManager.GetItemID(item), 1);
			}
			componentMiner.ComponentPlayer?.ComponentGui.DisplaySmallMessage(LanguageControl.Get("Behaviors", "ScrewDriversItemBehavior", reverse ? "StateReverse" : "StateNormal"), Color.White, blinking: true, playNotificationSound: false);
			Project.FindSubsystem<SubsystemAudio>().PlaySound("Audio/HammerCock", 1f, new Game.Random().Float(-0.1f, 0.1f), 0f, 0f);
			return true;
		}
		return false;
	}
}
