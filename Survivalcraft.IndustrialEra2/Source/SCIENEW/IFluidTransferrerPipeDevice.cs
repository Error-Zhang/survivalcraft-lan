using System;
using System.Collections.Generic;
using Game;

namespace SCIENEW;

public interface IFluidTransferrerPipeDevice
{
	Type GetTransferringFluidType();

	bool CanConnect(int value, CellFace neighbor, Terrain terrain);

	PipeFluidConnection GetConnection(HashSet<FluidTransferrer> fromTransferrers, HashSet<FluidTransferrer> toTransferrers, HashSet<FluidConnector> fromConnectors, HashSet<FluidConnector> toConnectors, float rate, Type transferringType, SubsystemFluidTransfer subsystemFluidTransfer);
}
