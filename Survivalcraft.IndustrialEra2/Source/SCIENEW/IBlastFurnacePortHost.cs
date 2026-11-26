namespace SCIENEW;

public interface IBlastFurnacePortHost : ITank
{
	bool IsBlastPortOpen { get; }
}
