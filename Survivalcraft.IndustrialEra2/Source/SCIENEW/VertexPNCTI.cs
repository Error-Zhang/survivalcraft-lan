using Engine;
using Engine.Graphics;

namespace SCIENEW;

public struct VertexPNCTI(Vector3 position, Vector3 normal, Color color, Vector2 texCoord, int instance)
{
	public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementSemantic.Position), new VertexElement(12, VertexElementFormat.Vector3, VertexElementSemantic.Normal), new VertexElement(24, VertexElementFormat.NormalizedByte4, VertexElementSemantic.Color), new VertexElement(28, VertexElementFormat.Vector2, VertexElementSemantic.TextureCoordinate), new VertexElement(36, VertexElementFormat.Single, VertexElementSemantic.Instance));

	public Vector3 Position = position;

	public Vector3 Normal = normal;

	public Color Color = color;

	public Vector2 TexCoord = texCoord;

	public int Instance = instance;
}
