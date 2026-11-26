using Engine;
using Engine.Graphics;
using Engine.Media;
using Game;

namespace SCIENEW.IconMaker;

public static class IconMakerRenderer
{
	private const int PreviewMaxDimension = 512;

	private static readonly PrimitivesRenderer3D s_primitives = new PrimitivesRenderer3D();

	private static readonly IconMakerBackgroundBatch2D s_backgroundBatch = new IconMakerBackgroundBatch2D();

	private static readonly IconMakerComposeBatch2D s_composeBatch = new IconMakerComposeBatch2D();

	private static RenderTarget2D? s_subjectRt;

	private static RenderTarget2D? s_backgroundRt;

	private static RenderTarget2D? s_outputRt;

	private static int s_subjectSize;

	private static int s_backgroundSize;

	private static int s_outputSize;

	public static RenderTarget2D Compose(IconMakerProject project, bool previewQuality = true)
	{
		int num = MathUtils.Clamp(project.OutputSize, 128, 2048);
		if (previewQuality)
		{
			num = MathUtils.Min(num, 512);
		}
		Vector2 vector = new Vector2(num);
		RenderTarget2D renderTarget2D = EnsureRt(ref s_subjectRt, ref s_subjectSize, num);
		RenderTarget2D renderTarget2D2 = EnsureRt(ref s_backgroundRt, ref s_backgroundSize, num);
		RenderTarget2D renderTarget2D3 = EnsureRt(ref s_outputRt, ref s_outputSize, num);
		Matrix matrix = CreateRtMatrix(num);
		RenderSubject(project, renderTarget2D, num, matrix);
		RenderBackground(project, renderTarget2D2, vector, matrix);
		RenderTarget2D renderTarget = Display.RenderTarget;
		Viewport viewport = Display.Viewport;
		try
		{
			Display.RenderTarget = renderTarget2D3;
			Display.Viewport = new Viewport(0, 0, num, num);
			Display.Clear(Color.Transparent, 1f, 0);
			s_composeBatch.Texture = renderTarget2D;
			s_composeBatch.SamplerState = SamplerState.LinearClamp;
			s_composeBatch.QueueQuad(Vector2.Zero, vector, 0f, Vector2.Zero, Vector2.One, Color.White);
			s_composeBatch.FlushWithProject(project, vector, renderTarget2D2, renderTarget2D, matrix);
			return renderTarget2D3;
		}
		finally
		{
			Display.RenderTarget = renderTarget;
			Display.Viewport = viewport;
		}
	}

	public static void ExportPng(IconMakerProject project, string filePath)
	{
		RenderTarget2D.Save(Compose(project, previewQuality: false), filePath, ImageFileFormat.Png, saveAlpha: true);
	}

	private static RenderTarget2D EnsureRt(ref RenderTarget2D? rt, ref int cachedSize, int size)
	{
		if (rt == null || cachedSize != size)
		{
			rt?.Dispose();
			rt = new RenderTarget2D(size, size, 1, ColorFormat.Rgba8888, DepthFormat.Depth24Stencil8);
			cachedSize = size;
		}
		return rt;
	}

	private static Matrix CreateRtMatrix(int size)
	{
		return new Matrix(2f / (float)size, 0f, 0f, 0f, 0f, -2f / (float)size, 0f, 0f, 0f, 0f, 1f, 0f, -1f, 1f, 0f, 1f);
	}

	private static void RenderBackground(IconMakerProject project, RenderTarget2D rt, Vector2 outputSize, Matrix matrix)
	{
		Texture2D texture = IconMakerAssetCatalog.LoadTexture(project.BackgroundTexturePath);
		RenderTarget2D renderTarget = Display.RenderTarget;
		Viewport viewport = Display.Viewport;
		try
		{
			Display.RenderTarget = rt;
			Display.Viewport = new Viewport(0, 0, rt.Width, rt.Height);
			Display.Clear(new Color(32, 32, 36), 1f, 0);
			s_backgroundBatch.Texture = texture;
			s_backgroundBatch.SamplerState = IconMakerBgSampler.ToSampler(project.BgSamplerMode);
			s_backgroundBatch.QueueQuad(Vector2.Zero, outputSize, 0f, Vector2.Zero, Vector2.One, Color.White);
			s_backgroundBatch.FlushWithProject(project, outputSize, matrix);
		}
		finally
		{
			Display.RenderTarget = renderTarget;
			Display.Viewport = viewport;
		}
	}

	private static void RenderSubject(IconMakerProject project, RenderTarget2D rt, int size, Matrix matrix)
	{
		RenderTarget2D renderTarget = Display.RenderTarget;
		Viewport viewport = Display.Viewport;
		try
		{
			Display.RenderTarget = rt;
			Display.Viewport = new Viewport(0, 0, size, size);
			Display.Clear(Color.Transparent, 1f, 0);
			if (project.SubjectKind == IconMakerSubjectKind.Block)
			{
				RenderBlockSubject(project, size);
			}
			else
			{
				RenderTextureSubject(project, size, matrix);
			}
		}
		finally
		{
			Display.RenderTarget = renderTarget;
			Display.Viewport = viewport;
		}
	}

	private static void RenderBlockSubject(IconMakerProject project, int size)
	{
		if (project.BlockValue != 0)
		{
			Block obj = BlocksManager.Blocks[Terrain.ExtractContents(project.BlockValue)];
			DrawBlockEnvironmentData drawBlockEnvironmentData = new DrawBlockEnvironmentData();
			Matrix matrix = Matrix.CreateLookAt(obj.GetIconViewOffset(project.BlockValue, drawBlockEnvironmentData), Vector3.Zero, Vector3.UnitY);
			float num = 1f;
			Matrix matrix2 = Matrix.CreateOrthographic(3.6f, 3.6f, -10f - num, 10f - num);
			drawBlockEnvironmentData.DrawBlockMode = DrawBlockMode.UI;
			drawBlockEnvironmentData.ViewProjectionMatrix = matrix * matrix2;
			float size2 = obj.GetIconViewScale(project.BlockValue, drawBlockEnvironmentData) * 2.2f * MathUtils.Max(project.SubjectScale, 0.05f);
			Vector3 iconBlockOffset = obj.GetIconBlockOffset(project.BlockValue, drawBlockEnvironmentData);
			iconBlockOffset.X += project.SubjectOffset.X * 0.35f;
			iconBlockOffset.Y += project.SubjectOffset.Y * 0.35f;
			Matrix matrix3 = Matrix.CreateTranslation(iconBlockOffset);
			obj.DrawBlock(s_primitives, project.BlockValue, Color.White, size2, ref matrix3, drawBlockEnvironmentData);
			s_primitives.Flush(Matrix.Identity);
			s_primitives.Flush(Matrix.Identity);
		}
	}

	private static void RenderTextureSubject(IconMakerProject project, int size, Matrix matrix)
	{
		Texture2D texture = IconMakerAssetCatalog.LoadTexture(project.TexturePath);
		float num = MathUtils.Max(project.SubjectScale, 0.05f);
		Vector2 vector = new Vector2(size) * 0.5f;
		Vector2 vector2 = new Vector2(size) * num * 0.85f;
		Vector2 vector3 = vector - vector2 * 0.5f + project.SubjectOffset * size * 0.15f;
		Vector2 corner = vector3 + vector2;
		TexturedBatch2D texturedBatch2D = new TexturedBatch2D();
		texturedBatch2D.Texture = texture;
		texturedBatch2D.SamplerState = SamplerState.LinearClamp;
		texturedBatch2D.QueueQuad(vector3, corner, 0f, Vector2.Zero, Vector2.One, Color.White);
		texturedBatch2D.Flush(matrix);
	}
}
