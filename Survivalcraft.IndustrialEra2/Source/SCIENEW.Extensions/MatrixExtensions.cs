using Engine;

namespace SCIENEW.Extensions;

public static class MatrixExtensions
{
	public static Matrix Invert(this Matrix matrix)
	{
		return Matrix.Invert(matrix);
	}

	public static Matrix Transpose(this Matrix matrix)
	{
		return Matrix.Transpose(matrix);
	}
}
