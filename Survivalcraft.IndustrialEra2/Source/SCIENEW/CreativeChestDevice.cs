using Engine;
using Game;
using GameEntitySystem;

namespace SCIENEW;

public class CreativeChestDevice : SixDirectionalDevice, IEntityDevice<ComponentCreativeChest>, IElectricElementDevice
{
	public enum Mode
	{
		Shoot,
		Dispense
	}

	public CreativeChestDevice()
		: base("CreativeChestDevice", 45, 46, 46)
	{
	}

	public string GetEntityName(int value)
	{
		return "CreativeChest";
	}

	public Widget GetWidget(IInventory inventory, ComponentCreativeChest component)
	{
		return new CreativeChestWidget(inventory, component);
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new CreativeChestElectricElement(subsystemElectricity, new Point3(x, y, z));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public override int GetFaceTextureSlot(int face, int value)
	{
		if (face == DevicesUtils.GetInnerData(value))
		{
			return 45;
		}
		if ((uint)(face - 4) <= 1u)
		{
			return 107;
		}
		return 46;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + -3;
	}

	public override DeviceBehavior[] GetBehaviors(Project project)
	{
		return new DeviceBehavior[1]
		{
			new CreativeChestDeviceBehavior(project)
		};
	}
}
