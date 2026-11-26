using Engine;
using Engine.Graphics;
using Game;

namespace SCIENEW;

public class TechTreeBlockIconWidget : Widget
{
	public Matrix m_viewMatrix;

	public int m_value;

	private PrimitivesRenderer3D m_primitivesRenderer3D;

	private RenderTarget2D m_renderTarget2D;

	public bool m_needRefresh = true;

	private int m_pendingRedrawFrames;

	public DrawBlockEnvironmentData DrawBlockEnvironmentData { get; set; }

	public Vector2 Size { get; set; }

	public float Depth { get; set; }

	public Color Color { get; set; }

	public Matrix? CustomViewMatrix { get; set; }

	public int Value
	{
		get
		{
			return m_value;
		}
		set
		{
			if (m_value == 0 || value != m_value)
			{
				m_value = value;
				if (UsesPrivateRasterPath())
				{
					m_needRefresh = true;
					m_pendingRedrawFrames = MathUtils.Max(m_pendingRedrawFrames, 2);
				}
				Block block = BlocksManager.Blocks[Contents];
				m_viewMatrix = Matrix.CreateLookAt(block.GetIconViewOffset(Value, DrawBlockEnvironmentData), new Vector3(0f, 0f, 0f), Vector3.UnitY);
			}
		}
	}

	public int Contents
	{
		get
		{
			return Terrain.ExtractContents(Value);
		}
		set
		{
			Value = Terrain.ReplaceContents(Value, value);
		}
	}

	public int Light
	{
		get
		{
			return Terrain.ExtractLight(Value);
		}
		set
		{
			Value = Terrain.ReplaceLight(Value, value);
		}
	}

	public int Data
	{
		get
		{
			return Terrain.ExtractData(Value);
		}
		set
		{
			Value = Terrain.ReplaceData(Value, value);
		}
	}

	public float Scale { get; set; }

	public TechTreeBlockIconWidget()
	{
		DrawBlockEnvironmentData = new DrawBlockEnvironmentData();
		Size = new Vector2(48f);
		IsHitTestVisible = false;
		ClampToBounds = true;
		Light = 15;
		Depth = 1f;
		Color = Color.White;
		Scale = 1f;
	}

	private bool UsesPrivateRasterPath()
	{
		if (!CustomViewMatrix.HasValue)
		{
			return !IEIconManager.CanUseSharedBlockIconRaster(DrawBlockEnvironmentData);
		}
		return true;
	}

	private void EnsurePrivateRasterResources()
	{
		if (m_renderTarget2D == null)
		{
			m_renderTarget2D = new RenderTarget2D((int)Size.X * 4, (int)Size.Y * 4, 1, ColorFormat.Rgba8888, DepthFormat.Depth24Stencil8);
			m_primitivesRenderer3D = new PrimitivesRenderer3D();
			m_needRefresh = true;
			m_pendingRedrawFrames = MathUtils.Max(m_pendingRedrawFrames, 2);
		}
	}

	public override void Update()
	{
		base.Update();
		if (UsesPrivateRasterPath() && m_pendingRedrawFrames > 0)
		{
			m_pendingRedrawFrames--;
			m_needRefresh = true;
		}
	}

	public override void Draw(DrawContext dc)
	{
		RenderTarget2D texture;
		if (!UsesPrivateRasterPath())
		{
			texture = IEIconManager.GetSharedBlockIconRaster(Value, DrawBlockEnvironmentData);
		}
		else
		{
			EnsurePrivateRasterResources();
			if (m_needRefresh)
			{
				m_needRefresh = false;
				Block obj = BlocksManager.Blocks[Contents];
				if (DrawBlockEnvironmentData.SubsystemTerrain == null)
				{
					_ = BlocksTexturesManager.DefaultBlocksTexture;
				}
				else
				{
					_ = DrawBlockEnvironmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture;
				}
				Matrix matrix = Matrix.CreateOrthographic(3.6f, 3.6f, -10f - 1f * Depth, 10f - 1f * Depth);
				DrawBlockEnvironmentData.DrawBlockMode = DrawBlockMode.UI;
				DrawBlockEnvironmentData.ViewProjectionMatrix = (CustomViewMatrix ?? m_viewMatrix) * matrix;
				float size = BlocksManager.Blocks[Contents].GetIconViewScale(Value, DrawBlockEnvironmentData) * 2.2f;
				Matrix matrix2 = (CustomViewMatrix.HasValue ? (Matrix.Identity * Matrix.CreateScale(10f, 10f, 10f)) : Matrix.CreateTranslation(BlocksManager.Blocks[Contents].GetIconBlockOffset(Value, DrawBlockEnvironmentData)));
				obj.DrawBlock(m_primitivesRenderer3D, Value, GlobalColorTransform, size, ref matrix2, DrawBlockEnvironmentData);
				RenderTarget2D renderTarget = Display.RenderTarget;
				Display.RenderTarget = m_renderTarget2D;
				Display.Clear(Color.Transparent, 1f, 0);
				try
				{
					m_primitivesRenderer3D.Flush(Matrix.Identity);
				}
				finally
				{
					Display.RenderTarget = renderTarget;
				}
			}
			texture = m_renderTarget2D;
		}
		Matrix m = GlobalTransform;
		Vector2 v = Vector2.Zero;
		Vector2 v2 = new Vector2(ActualSize.X, 0f);
		Vector2 v3 = ActualSize;
		Vector2 v4 = new Vector2(0f, ActualSize.Y);
		Vector2.Transform(ref v, ref m, out var result);
		Vector2.Transform(ref v2, ref m, out var result2);
		Vector2.Transform(ref v3, ref m, out var result3);
		Vector2.Transform(ref v4, ref m, out var result4);
		dc.PrimitivesRenderer2D.TexturedBatch(texture, useAlphaTest: false, 0, null, null, null, SamplerState.LinearClamp).QueueQuad(result, result2, result3, result4, 1f, Vector2.Zero, Vector2.UnitX, Vector2.One, Vector2.UnitY, Color.White);
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		IsDrawRequired = true;
		DesiredSize = Size;
	}
}
