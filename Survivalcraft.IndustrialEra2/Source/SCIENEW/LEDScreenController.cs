using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;

namespace SCIENEW;

public class LEDScreenController
{
	public Dictionary<CellFace, int> m_cells = new Dictionary<CellFace, int>();

	public int ID => GetHashCode();

	public int ControlledCount => m_cells.Count;

	public int MountFace { get; private set; }

	public Point3 AnchorPoint { get; private set; }

	public LEDScreenInscribedRect InscribedRect { get; private set; }

	public LEDScreenController(Dictionary<CellFace, int> cells)
	{
		m_cells = cells;
		CellFace lexicographicMinCellFace = GetLexicographicMinCellFace(cells.Keys);
		MountFace = lexicographicMinCellFace.Face;
		AnchorPoint = lexicographicMinCellFace.Point;
		InscribedRect = LEDScreenGroupingUtils.ComputeMaxInscribedRect(GetCellPoints(), MountFace);
	}

	public CellFace GetAnchorCellFace()
	{
		return new CellFace(AnchorPoint.X, AnchorPoint.Y, AnchorPoint.Z, MountFace);
	}

	private static CellFace GetLexicographicMinCellFace(IEnumerable<CellFace> cells)
	{
		CellFace result = default;
		bool flag = true;
		foreach (CellFace cell in cells)
		{
			if (flag || ComparePoint(cell.Point, result.Point) < 0)
			{
				result = cell;
				flag = false;
			}
		}
		return result;
	}

	private static int ComparePoint(Point3 a, Point3 b)
	{
		if (a.X != b.X)
		{
			return a.X.CompareTo(b.X);
		}
		if (a.Y != b.Y)
		{
			return a.Y.CompareTo(b.Y);
		}
		return a.Z.CompareTo(b.Z);
	}

	public Point3[] GetCellPoints()
	{
		return m_cells.Keys.Select((CellFace c) => c.Point).ToArray();
	}
}
