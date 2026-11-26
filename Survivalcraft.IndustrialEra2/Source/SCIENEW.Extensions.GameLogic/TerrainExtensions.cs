using Game;
using SCIENEW.Utils;

namespace SCIENEW.Extensions.GameLogic;

public static class TerrainExtensions
{
	public static bool IsValid(this TerrainChunk chunk)
	{
		return TerrainUtils.IsValid(chunk);
	}
}
