using System.Collections.Generic;

namespace SCIENEW.Ponder;

public interface IPonderBlockSet<T> : IPonderSet where T : IPonderBlock
{
	List<T> PonderBlocks { get; }
}
