using System;
using Engine;
using Engine.Graphics;
using Game;
using SCIENEW.Extensions;
using SCIENEW.Extensions.Graphics;

namespace SCIENEW;

public class BottleItem : MeshItem
{
	public BlockMesh m_standaloneBlockMeshWithNormal = new BlockMesh();

	public BlockMesh m_liquidMesh = new BlockMesh();

	public BlockMesh m_liquidMesh2 = new BlockMesh();

	public BlockMesh m_liquidBottleMesh = new BlockMesh();

	public BlockMesh m_liquidBottleMeshWithNormal = new BlockMesh();

	public Color LiquidColor;

	public Texture2D WhiteTexture;

	public BottleItem(string id, Color liquidColor)
		: base(id)
	{
		m_standaloneBlockMeshWithNormal.AppendMeshWithNormal("Models/GlassBottle", "GlassBottle", Matrix.CreateScale(1.5f), Matrix.Identity, Color.White);
		m_standaloneBlockMesh.AppendMesh("Models/GlassBottle", "GlassBottle", Matrix.CreateScale(1.5f), Matrix.Identity, Color.White);
		m_liquidMesh.AppendMesh("Models/GlassBottle", "Liquid", Matrix.CreateScale(1.5f), Matrix.Identity, Color.White, doubleSided: true);
		m_liquidMesh2.AppendMesh("Models/GlassBottle", "Liquid2", Matrix.CreateScale(1.5f), Matrix.Identity, Color.White);
		m_liquidBottleMesh.AppendMesh("Models/GlassBottle", "Liquid_Bottle", Matrix.CreateScale(1.5f), Matrix.Identity, Color.White);
		m_liquidBottleMeshWithNormal.AppendMeshWithNormal("Models/GlassBottle", "Liquid_Bottle", Matrix.CreateScale(1.5f), Matrix.Identity, Color.White);
		Texture = ContentManager.Get<Texture2D>("Textures/Iron");
		WhiteTexture = ContentManager.Get<Texture2D>("Textures/White");
		LiquidColor = liquidColor;
	}

	public override int GetDisplayOrder(int value)
	{
		return base.GetDisplayOrder(value) + 1;
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		if (environmentData.DrawBlockMode == DrawBlockMode.UI)
		{
			if (LiquidColor == Color.Transparent)
			{
				DrawMeshBlock(primitivesRenderer, m_standaloneBlockMeshWithNormal, Texture, color, size, ref matrix, environmentData);
				return;
			}
			DrawMeshBlock(primitivesRenderer, m_liquidBottleMeshWithNormal, Texture, color, size, ref matrix, environmentData);
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_liquidMesh, WhiteTexture, color * LiquidColor, size, ref matrix, environmentData);
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_liquidMesh2, WhiteTexture, color * LiquidColor, size, ref matrix, environmentData);
		}
		else if (LiquidColor == Color.Transparent)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, Texture, color * new Color(100, 100, 100, 64), size, ref matrix, environmentData);
		}
		else
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_liquidBottleMesh, Texture, color * new Color(100, 100, 100, 64), size, ref matrix, environmentData);
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_liquidMesh, WhiteTexture, color * LiquidColor, size, ref matrix, environmentData);
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_liquidMesh2, WhiteTexture, color * LiquidColor, size, ref matrix, environmentData);
		}
	}

	public static void DrawLiquidBlock(PrimitivesRenderer3D primitivesRenderer, BlockMesh blockMesh, Texture2D texture, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		environmentData = environmentData ?? BlocksManager.m_defaultEnvironmentData;
		float num = LightingManager.LightIntensityByLightValue[environmentData.Light];
		Vector4 v = new Vector4(color);
		v.X *= num;
		v.Y *= num;
		v.Z *= num;
		Vector4 v2 = new Vector4(Color.White);
		v2.X *= num;
		v2.Y *= num;
		v2.Z *= num;
		TexturedBatch3D texturedBatch3D = primitivesRenderer.TexturedBatch(texture, useAlphaTest: true, 0, null, RasterizerState.CullCounterClockwiseScissor, null, SamplerState.PointClamp);
		bool flag = false;
		Matrix m = ((!environmentData.ViewProjectionMatrix.HasValue) ? matrix : (matrix * environmentData.ViewProjectionMatrix.Value));
		if (size != 1f)
		{
			m = Matrix.CreateScale(size) * m;
		}
		if (m.M14 != 0f || m.M24 != 0f || m.M34 != 0f || m.M44 != 1f)
		{
			flag = true;
		}
		int count = blockMesh.Vertices.Count;
		BlockMeshVertex[] array = blockMesh.Vertices.Array;
		int count2 = blockMesh.Indices.Count;
		int[] array2 = blockMesh.Indices.Array;
		DynamicArray<VertexPositionColorTexture> triangleVertices = texturedBatch3D.TriangleVertices;
		int count3 = triangleVertices.Count;
		int count4 = triangleVertices.Count;
		triangleVertices.Count += count;
		for (int i = 0; i < count; i++)
		{
			BlockMeshVertex blockMeshVertex = array[i];
			if (flag)
			{
				Vector4 v3 = new Vector4(blockMeshVertex.Position, 1f);
				Vector4.Transform(ref v3, ref m, out v3);
				float num2 = 1f / v3.W;
				blockMeshVertex.Position = new Vector3(v3.X * num2, v3.Y * num2, v3.Z * num2);
			}
			else
			{
				Vector3.Transform(ref blockMeshVertex.Position, ref m, out blockMeshVertex.Position);
			}
			Vector3 v4 = new Vector3(blockMeshVertex.Color);
			Vector3 v5 = new Vector3(-1f, -1f, -1f).Normalize();
			float x = 1f - MathF.Abs(v4.Dot(v5));
			x = Math.Clamp(MathF.Pow(x, 1.5f) * 2f, 0f, 1f);
			Vector4 rgba = Vector4.Lerp(v, v2, x);
			Color color2 = new Color(rgba);
			triangleVertices.Array[count4++] = new VertexPositionColorTexture(blockMeshVertex.Position, color2, blockMeshVertex.TextureCoordinates);
		}
		DynamicArray<int> triangleIndices = texturedBatch3D.TriangleIndices;
		int count5 = triangleIndices.Count;
		triangleIndices.Count += count2;
		for (int j = 0; j < count2; j++)
		{
			triangleIndices.Array[count5++] = count3 + array2[j];
		}
	}

	public static void DrawMeshBlock(PrimitivesRenderer3D primitivesRenderer, BlockMesh blockMesh, Texture2D texture, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		environmentData = environmentData ?? BlocksManager.m_defaultEnvironmentData;
		float num = LightingManager.LightIntensityByLightValue[environmentData.Light];
		Vector4 vector = new Vector4(color);
		vector.X *= num;
		vector.Y *= num;
		vector.Z *= num;
		_ = vector == Vector4.One;
		TexturedBatch3D texturedBatch3D = primitivesRenderer.TexturedBatch(texture, useAlphaTest: true, 0, null, RasterizerState.CullCounterClockwiseScissor, null, SamplerState.PointClamp);
		bool flag = false;
		Matrix m = ((!environmentData.ViewProjectionMatrix.HasValue) ? matrix : (matrix * environmentData.ViewProjectionMatrix.Value));
		if (size != 1f)
		{
			m = Matrix.CreateScale(size) * m;
		}
		if (m.M14 != 0f || m.M24 != 0f || m.M34 != 0f || m.M44 != 1f)
		{
			flag = true;
		}
		int count = blockMesh.Vertices.Count;
		BlockMeshVertex[] array = blockMesh.Vertices.Array;
		int count2 = blockMesh.Indices.Count;
		int[] array2 = blockMesh.Indices.Array;
		DynamicArray<VertexPositionColorTexture> triangleVertices = texturedBatch3D.TriangleVertices;
		int count3 = triangleVertices.Count;
		int count4 = triangleVertices.Count;
		triangleVertices.Count += count;
		for (int i = 0; i < count; i++)
		{
			BlockMeshVertex blockMeshVertex = array[i];
			if (flag)
			{
				Vector4 v = new Vector4(blockMeshVertex.Position, 1f);
				Vector4.Transform(ref v, ref m, out v);
				float num2 = 1f / v.W;
				blockMeshVertex.Position = new Vector3(v.X * num2, v.Y * num2, v.Z * num2);
			}
			else
			{
				Vector3.Transform(ref blockMeshVertex.Position, ref m, out blockMeshVertex.Position);
			}
			Vector3 v2 = new Vector3(blockMeshVertex.Color);
			Vector3 v3 = new Vector3(-1f, -1f, -1f).Normalize();
			float x = 1f - MathF.Abs(v2.Dot(v3));
			x = Math.Clamp(MathF.Pow(x, 1.5f) * 1.5f, 0f, 1f);
			Color color2 = new Color(x * vector.X, x * vector.Y, x * vector.Z, x * vector.W);
			triangleVertices.Array[count4++] = new VertexPositionColorTexture(blockMeshVertex.Position, color2, blockMeshVertex.TextureCoordinates);
		}
		DynamicArray<int> triangleIndices = texturedBatch3D.TriangleIndices;
		int count5 = triangleIndices.Count;
		triangleIndices.Count += count2;
		for (int j = 0; j < count2; j++)
		{
			triangleIndices.Array[count5++] = count3 + array2[j];
		}
	}

	public override int GetMaxStacking(int value)
	{
		return 40;
	}
}
