using System.Collections.Generic;

namespace SCIENEW.ProductionIO;

public interface IInventoryProductionSlots
{
	IEnumerable<int> GetSlotIndices(ProductionSlotKind kind);
}
