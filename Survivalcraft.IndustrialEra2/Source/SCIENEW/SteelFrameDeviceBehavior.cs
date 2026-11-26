using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class SteelFrameDeviceBehavior : DeviceBehavior
{
	private readonly BiDictionary<SteelFrameDevice, string> m_frameToContainsMap = new BiDictionary<SteelFrameDevice, string>();

	public SteelFrameDeviceBehavior(Project project)
		: base(project)
	{
		SteelFrameDevice[] devices = DevicesBlockManager.GetDevices<SteelFrameDevice>();
		foreach (SteelFrameDevice steelFrameDevice in devices)
		{
			if (steelFrameDevice.m_containsWireID != null)
			{
				m_frameToContainsMap.Add(steelFrameDevice, steelFrameDevice.m_containsWireID);
			}
		}
	}

	public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
	{
		int activeBlockValue = componentMiner.ActiveBlockValue;
		if ((Terrain.ExtractContents(activeBlockValue) != BaseDeviceBlock.Index || !m_frameToContainsMap.ContainsValue(DevicesBlockManager.GetDeviceCraftingID(activeBlockValue))) && BlocksManager.Blocks[Terrain.ExtractContents(activeBlockValue)].IsPlaceable_(activeBlockValue))
		{
			return false;
		}
		int value = raycastResult.Value;
		if (DevicesBlockManager.GetDevice(value) is SteelFrameDevice { m_containsWireID: not null } steelFrameDevice)
		{
			Project.FindSubsystem<SubsystemPickables>(throwOnError: true).AddPickable(DevicesBlockManager.GetDeviceValue(steelFrameDevice.m_containsWireID, 0), 1, new Vector3(raycastResult.CellFace.Point) + new Vector3(0.5f), null, null);
		}
		if (Terrain.ExtractContents(activeBlockValue) == BaseDeviceBlock.Index && m_frameToContainsMap.TryGetBySecond(DevicesBlockManager.GetDevice(activeBlockValue).GetCraftingId(), out SteelFrameDevice first))
		{
			componentMiner.m_subsystemTerrain.ChangeCell(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z, DevicesBlockManager.GetDeviceValue(first.GetCraftingId(), DevicesUtils.GetInnerData(value)));
			componentMiner.RemoveActiveTool(1);
			Project.FindSubsystem<SubsystemAudio>(throwOnError: true).PlaySound("Audio/BlockPlaced", 1f, 0.3f, new Vector3(raycastResult.CellFace.Point), 2.5f, autoDelay: true);
			return true;
		}
		if (DevicesBlockManager.GetDevice(value) is SteelFrameDevice { m_containsWireID: not null })
		{
			componentMiner.m_subsystemTerrain.ChangeCell(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z, DevicesBlockManager.GetDeviceValue(SteelFrameDevice.GetCraftingId(Materials.Plastic), DevicesUtils.GetInnerData(value)));
			Project.FindSubsystem<SubsystemAudio>(throwOnError: true).PlaySound("Audio/BlockPlaced", 1f, 0.3f, new Vector3(raycastResult.CellFace.Point), 2.5f, autoDelay: true);
			return true;
		}
		return false;
	}
}
