using Engine;

namespace SCIENEW;

public class MachineShellNBlock : DefaultNBlock
{
	public MachineShellNBlock(string id, int slot)
		: base(id, IndustrialModLoader.IETexture, Color.White, slot)
	{
	}
}
