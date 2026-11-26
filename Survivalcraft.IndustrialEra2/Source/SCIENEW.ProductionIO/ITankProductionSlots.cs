using System.Collections.Generic;

namespace SCIENEW.ProductionIO;

public interface ITankProductionSlots
{
	IEnumerable<int> GetSlotIndices(ProductionSlotKind kind);
}
