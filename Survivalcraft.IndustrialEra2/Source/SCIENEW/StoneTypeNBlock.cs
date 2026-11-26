using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class StoneTypeNBlock : DefaultNBlock
{
	public int ExplosionResilience;

	public int DigResilience;

	public StoneTypeNBlock(string id, Texture2D texture, Color color, int faceTex, int explosionResilience = 2000, int digResilience = 24)
		: base(id, texture, color, faceTex)
	{
		ExplosionResilience = explosionResilience;
		DigResilience = digResilience;
	}

	public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
	{
		return "Stone";
	}

	public override float GetExplosionResilience(int value)
	{
		return ExplosionResilience;
	}

	public override float GetDigResilience(int value)
	{
		return DigResilience;
	}
}
