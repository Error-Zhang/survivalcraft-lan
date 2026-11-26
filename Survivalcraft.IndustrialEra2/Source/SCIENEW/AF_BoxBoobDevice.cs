using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class AF_BoxBoobDevice : ModelDevice, IElectricElementDevice
{
	public AF_BoxBoobDevice()
		: base("AF_BoxBoobDevice", "Models/AF_BoxGenerator", "AF_BoxGenerator", ContentManager.Get<Texture2D>("Textures/AF_BoxBoob"), Color.White)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 10086;
	}

	public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
	{
		return new AF_BoxBoobElectricElement(subsystemElectricity, new Point3(x, y, z));
	}

	public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
	{
		return ElectricConnectorType.Input;
	}

	public int GetConnectionMask(int value)
	{
		return int.MaxValue;
	}

	public override bool IsDisplay()
	{
		return IndustrialModLoader.IsAprilFools;
	}
}
