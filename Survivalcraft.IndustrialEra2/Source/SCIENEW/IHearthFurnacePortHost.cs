namespace SCIENEW;

public interface IHearthFurnacePortHost : ITank
{
	bool IsInputOpen { get; }

	bool IsOutputOpen { get; }

	int MoltenSteelTankIndex { get; }
}
