namespace SCIENEW.Ponder;

public class ProjectilePonderBlockHandle
{
	public ProjectilePonderBlock? Instance;

	public ProjectilePonderBlockHandle()
	{
		Instance = null;
	}

	public ProjectilePonderBlockHandle(ProjectilePonderBlock ponderBlock)
	{
		Instance = ponderBlock;
	}
}
