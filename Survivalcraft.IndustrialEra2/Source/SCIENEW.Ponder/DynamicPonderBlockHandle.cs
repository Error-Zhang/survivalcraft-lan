namespace SCIENEW.Ponder;

public class DynamicPonderBlockHandle
{
	public DynamicPonderBlock? Instance;

	public DynamicPonderBlockHandle()
	{
		Instance = null;
	}

	public DynamicPonderBlockHandle(DynamicPonderBlock ponderBlock)
	{
		Instance = ponderBlock;
	}
}
