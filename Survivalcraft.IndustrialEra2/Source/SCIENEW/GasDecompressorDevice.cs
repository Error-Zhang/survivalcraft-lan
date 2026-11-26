namespace SCIENEW;

public class GasDecompressorDevice : FourDirectionalDevice
{
	public GasDecompressorDevice()
		: base("GasDecompressorDevice", 197, 108, 108, 99, 99)
	{
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override bool IsDisplay()
	{
		return false;
	}
}
