using Engine;

namespace SCIENEW.Ponder;

public class StaticPonderBlock : IPonderBlock
{
	public Point3 m_staticPosition;

	public int m_value;

	public int Value => m_value;

	public Vector3 Position => new Vector3(m_staticPosition);
}
