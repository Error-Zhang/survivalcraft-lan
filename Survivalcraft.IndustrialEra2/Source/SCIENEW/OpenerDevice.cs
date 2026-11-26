namespace SCIENEW;

public class OpenerDevice : FourDirectionalDevice
{
	public OpenerDevice()
		: base("OpenerDevice", 210, 99, 99, 99, 99)
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
