namespace SCIENEW.Ponder;

public class PickablePonderBlockHandle
{
	public PickablePonderBlock? Instance;

	public PickablePonderBlockHandle()
	{
		Instance = null;
	}

	public PickablePonderBlockHandle(PickablePonderBlock ponderBlock)
	{
		Instance = ponderBlock;
	}
}
