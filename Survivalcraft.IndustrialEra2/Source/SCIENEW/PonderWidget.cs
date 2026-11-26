using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using SCIENEW.Ponder;
using SCIENEW.Utils;

namespace SCIENEW;

public class PonderWidget : Widget
{
	public Vector2? m_dragStartPoint;

	public Vector2 m_rotationSpeed;

	protected bool m_onlyRotateY;

	public Project? Project;

	public SCIENEW.Ponder.Ponder m_ponder;

	public SubsystemTime m_subsystemTime;

	public CoroutineSchedulerOrder m_coroutineSchedulerOrder = new CoroutineSchedulerOrder();

	public PonderCamera m_ponderCamera = new PonderCamera(null);

	public Vector3 EulerAngles { get; set; }

	public bool OnlyRotateY
	{
		get
		{
			return m_onlyRotateY;
		}
		set
		{
			m_onlyRotateY = value;
			if (m_onlyRotateY)
			{
				EulerAngles = new Vector3(MathUtils.DegToRad(30f), EulerAngles.Y, EulerAngles.Z);
			}
		}
	}

	public Vector2 Size { get; set; }

	public float Range { get; set; } = 16f;

	public float ViewportSize { get; set; } = 16f;

	public bool Paused { get; set; }

	public PonderWidget()
	{
		ClampToBounds = true;
		Size = new Vector2(float.PositiveInfinity);
		EulerAngles = new Vector3(MathUtils.DegToRad(30f), MathUtils.DegToRad(45f), 0f);
		OnlyRotateY = true;
	}

	public void Setup(Project project, IEnumerator ponderEvents)
	{
		if (m_ponder != null)
		{
			m_ponder.Dispose();
		}
		m_ponder = null;
		Project = project;
		EulerAngles = new Vector3(MathUtils.DegToRad(30f), MathUtils.DegToRad(45f), 0f);
		m_rotationSpeed = Vector2.Zero;
		float range = (ViewportSize = 16f);
		Range = range;
		Paused = false;
		m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		m_ponder = new SCIENEW.Ponder.Ponder(Project);
		m_coroutineSchedulerOrder.Reset();
		m_coroutineSchedulerOrder.AddCoroutine(ponderEvents);
	}

	public void Pause()
	{
		Paused = true;
	}

	public void Resume()
	{
		Paused = false;
	}

	public void PauseCoroutine()
	{
		m_coroutineSchedulerOrder.PauseAll();
	}

	public void ResumeCoroutine()
	{
		m_coroutineSchedulerOrder.ResumeAll();
	}

	public override void Draw(DrawContext dc)
	{
		Vector3 vector = new Vector3(8.5f, 2f, 8.5f);
		Matrix m = Matrix.CreateFromYawPitchRoll(EulerAngles.Y, EulerAngles.X, EulerAngles.Z);
		Vector3 vector2 = Vector3.UnitZ.Transform(m);
		Vector3 vector3 = Vector3.UnitY.Transform(m);
		Vector3 cameraRight = vector2.Cross(vector3);
		Vector3 vector4 = vector - vector2 * Range;
		Matrix viewMatrix = Matrix.CreateLookAt(vector4, vector, vector3);
		Matrix projectionMatrix = Matrix.CreateOrthographic(ViewportSize, ViewportSize * ActualSize.Y / ActualSize.X, 0.1f, 1000f);
		Matrix viewportMatrix = MatrixUtils.CreateScaleTranslation(ActualSize.X, 0f - ActualSize.Y, ActualSize.X / 2f, ActualSize.Y / 2f) * GlobalTransform * MatrixUtils.CreateScaleTranslation(2f / (float)Display.Viewport.Width, -2f / (float)Display.Viewport.Height, -1f, 1f);
		m_ponderCamera.CameraPosition = vector4;
		m_ponderCamera.CameraDirection = vector2;
		m_ponderCamera.CameraUp = vector3;
		m_ponderCamera.CameraRight = cameraRight;
		m_ponderCamera.m_viewMatrix = viewMatrix;
		m_ponderCamera.m_projectionMatrix = projectionMatrix;
		m_ponderCamera.m_viewportMatrix = viewportMatrix;
		m_ponder.Draw(m_ponderCamera);
	}

	public override void Update()
	{
		if (!Paused)
		{
			m_ponder.Update();
			m_coroutineSchedulerOrder.UpdateCoroutines();
		}
		UpdateDrag();
	}

	public void UpdateDrag()
	{
		if (Input.Tap.HasValue && HitTestGlobal(Input.Tap.Value) == this)
		{
			m_dragStartPoint = Input.Tap;
		}
		if (Input.Press.HasValue)
		{
			if (m_dragStartPoint.HasValue)
			{
				Vector2 vector = ScreenToWidget(Input.Press.Value) - ScreenToWidget(m_dragStartPoint.Value);
				Vector2 vector2 = new Vector2(-0.01f * vector.X, 0.01f * vector.Y);
				if (Time.FrameDuration > 0f)
				{
					m_rotationSpeed = vector2 / Time.FrameDuration;
				}
				ApplyRotation(vector2);
				m_dragStartPoint = Input.Press;
			}
		}
		else
		{
			m_dragStartPoint = null;
			ApplyRotation(m_rotationSpeed * Time.FrameDuration);
			m_rotationSpeed *= MathF.Pow(0.1f, Time.FrameDuration);
		}
	}

	public void ApplyRotation(Vector2 angles)
	{
		Vector3 eulerAngles = EulerAngles;
		eulerAngles.Y = EulerAngles.Y + angles.X;
		EulerAngles = eulerAngles;
		if (!OnlyRotateY)
		{
			eulerAngles = EulerAngles;
			eulerAngles.X = EulerAngles.X + angles.Y;
			EulerAngles = eulerAngles;
		}
		float num = MathUtils.DegToRad(89f);
		eulerAngles = EulerAngles;
		eulerAngles.X = Math.Clamp(EulerAngles.X, 0f - num, num);
		EulerAngles = eulerAngles;
	}

	public override void MeasureOverride(Vector2 parentAvailableSize)
	{
		IsDrawRequired = true;
		DesiredSize = Size;
	}

	public IEnumerator Idle(float seconds)
	{
		yield return new WaitForSubsystemTimeCondition(seconds, m_subsystemTime);
	}

	public IEnumerator RotateYLerp(float angleDegrees, float duration)
	{
		float elapsed = 0f;
		float target = MathUtils.DegToRad(angleDegrees) + EulerAngles.Y;
		float prev = EulerAngles.Y;
		Vector3 eulerAngles;
		while (elapsed < duration)
		{
			float gameTimeDelta = m_subsystemTime.GameTimeDelta;
			elapsed += gameTimeDelta;
			PonderWidget ponderWidget = this;
			eulerAngles = EulerAngles;
			eulerAngles.Y = MathUtils.Lerp(prev, target, elapsed / duration);
			ponderWidget.EulerAngles = eulerAngles;
			yield return default(WaitForFrameCondition);
		}
		PonderWidget ponderWidget2 = this;
		eulerAngles = EulerAngles;
		eulerAngles.Y = target;
		ponderWidget2.EulerAngles = eulerAngles;
	}

	public IEnumerator RangeTo(float target, float duration)
	{
		float elapsed = 0f;
		float previousRange = Range;
		while (elapsed < duration)
		{
			float gameTimeDelta = m_subsystemTime.GameTimeDelta;
			elapsed += gameTimeDelta;
			float f = MathUtils.Saturate(elapsed / duration);
			Range = MathUtils.Lerp(previousRange, target, f);
			yield return default(WaitForFrameCondition);
		}
		Range = target;
	}

	public IEnumerator ViewportSizeTo(float target, float duration)
	{
		float elapsed = 0f;
		float previousSize = ViewportSize;
		while (elapsed < duration)
		{
			float gameTimeDelta = m_subsystemTime.GameTimeDelta;
			elapsed += gameTimeDelta;
			float f = MathUtils.Saturate(elapsed / duration);
			ViewportSize = MathUtils.Lerp(previousSize, target, f);
			yield return default(WaitForFrameCondition);
		}
		ViewportSize = target;
	}

	public IEnumerator RotateYBezier(float angleDegrees, float duration)
	{
		float elapsed = 0f;
		float target = MathUtils.DegToRad(angleDegrees) + EulerAngles.Y;
		float prev = EulerAngles.Y;
		Vector3 eulerAngles;
		while (elapsed < duration)
		{
			float gameTimeDelta = m_subsystemTime.GameTimeDelta;
			elapsed += gameTimeDelta;
			PonderWidget ponderWidget = this;
			eulerAngles = EulerAngles;
			eulerAngles.Y = EaseUtils.BezierEaseInOut(prev, target, elapsed / duration);
			ponderWidget.EulerAngles = eulerAngles;
			yield return default(WaitForFrameCondition);
		}
		PonderWidget ponderWidget2 = this;
		eulerAngles = EulerAngles;
		eulerAngles.Y = target;
		ponderWidget2.EulerAngles = eulerAngles;
	}

	public IEnumerator AddStaticBlock(Point3 position, int blockValue)
	{
		m_ponder.StaticPonderBlockSet.SetStaticBlock(position, blockValue);
		yield break;
	}

	public IEnumerator AddStaticBlockEaseIn(Point3 position, int blockValue, float duration = 0.5f)
	{
		float elapsed = 0f;
		Matrix startMat = Matrix.CreateTranslation(new Vector3(position) + new Vector3(0.5f, 1f, 0.5f));
		Matrix endMat = Matrix.CreateTranslation(new Vector3(position) + new Vector3(0.5f, 0.5f, 0.5f));
		Color startColor = new Color(0, 0, 0, 0);
		Color endColor = new Color(255, 255, 255, 255);
		DynamicPonderBlock dynamicPonderBlock = m_ponder.DynamicPonderBlockSet.Add(blockValue, startMat, startColor);
		while (elapsed < duration)
		{
			float gameTimeDelta = m_subsystemTime.GameTimeDelta;
			elapsed += gameTimeDelta;
			float f = EaseUtils.ExpEaseOut(1f, 0f, elapsed / duration);
			Matrix matrix = Matrix.Lerp(startMat, endMat, f);
			Color color = Color.Lerp(startColor, endColor, f);
			dynamicPonderBlock.Matrix = matrix;
			dynamicPonderBlock.Color = color;
			yield return default(WaitForFrameCondition);
		}
		m_ponder.DynamicPonderBlockSet.Remove(dynamicPonderBlock);
		m_ponder.StaticPonderBlockSet.SetStaticBlock(position, blockValue);
	}

	public IEnumerator AddStaticBlockRange(Dictionary<Point3, int> blocks)
	{
		foreach (KeyValuePair<Point3, int> block in blocks)
		{
			m_ponder.StaticPonderBlockSet.SetStaticBlock(block.Key, block.Value);
		}
		yield break;
	}

	public IEnumerator AddStaticBlockRangeEaseIn(Dictionary<Point3, int> blocks, float duration = 0.5f)
	{
		float elapsed = 0f;
		List<Matrix> startMats = new List<Matrix>();
		List<Matrix> endMats = new List<Matrix>();
		startMats.AddRange(blocks.Select((KeyValuePair<Point3, int> block) => Matrix.CreateTranslation(new Vector3(block.Key) + new Vector3(0.5f, 1f, 0.5f))));
		endMats.AddRange(blocks.Select((KeyValuePair<Point3, int> block) => Matrix.CreateTranslation(new Vector3(block.Key) + new Vector3(0.5f, 0.5f, 0.5f))));
		Color startColor = new Color(0, 0, 0, 0);
		Color endColor = new Color(255, 255, 255, 255);
		List<DynamicPonderBlock> dynamicPonderBlocks = new List<DynamicPonderBlock>();
		int num = 0;
		foreach (KeyValuePair<Point3, int> block in blocks)
		{
			dynamicPonderBlocks.Add(m_ponder.DynamicPonderBlockSet.Add(block.Value, startMats[num], startColor));
			num++;
		}
		while (elapsed < duration)
		{
			float gameTimeDelta = m_subsystemTime.GameTimeDelta;
			elapsed += gameTimeDelta;
			float f = EaseUtils.ExpEaseOut(1f, 0f, elapsed / duration);
			Color color = Color.Lerp(startColor, endColor, f);
			for (int num2 = 0; num2 < dynamicPonderBlocks.Count; num2++)
			{
				Matrix matrix = Matrix.Lerp(startMats[num2], endMats[num2], f);
				dynamicPonderBlocks[num2].Matrix = matrix;
				dynamicPonderBlocks[num2].Color = color;
			}
			yield return default(WaitForFrameCondition);
		}
		foreach (DynamicPonderBlock item in dynamicPonderBlocks)
		{
			m_ponder.DynamicPonderBlockSet.Remove(item);
		}
		foreach (KeyValuePair<Point3, int> block2 in blocks)
		{
			m_ponder.StaticPonderBlockSet.SetStaticBlock(block2.Key, block2.Value);
		}
	}

	public IEnumerator RemoveStaticBlock(Point3 position)
	{
		m_ponder.StaticPonderBlockSet.RemoveStaticBlock(position);
		yield break;
	}

	public IEnumerator RemoveStaticBlockRange(HashSet<Point3> positions)
	{
		foreach (Point3 position in positions)
		{
			m_ponder.StaticPonderBlockSet.RemoveStaticBlock(position);
		}
		yield break;
	}

	public IEnumerator AddDynamicBlock(int blockValue, Matrix matrix, Color color, Action<DynamicPonderBlock> createdCallback)
	{
		DynamicPonderBlock obj = m_ponder.DynamicPonderBlockSet.Add(blockValue, matrix, color);
		createdCallback?.Invoke(obj);
		yield break;
	}

	public IEnumerator RemoveDynamicBlock(DynamicPonderBlock block)
	{
		m_ponder.DynamicPonderBlockSet.Remove(block);
		yield break;
	}

	public IEnumerator RemoveDynamicBlock(DynamicPonderBlockHandle block)
	{
		m_ponder.DynamicPonderBlockSet.Remove(block.Instance);
		yield break;
	}

	public IEnumerator AddPickableBlock(int blockValue, Vector3 position, Vector3 velocity, Action<PickablePonderBlock> createdCallback, Matrix? stuckMatrix = null, object tag = null)
	{
		PickablePonderBlock obj = m_ponder.PickablePonderBlockSet.Add(blockValue, position, velocity, stuckMatrix, tag);
		createdCallback?.Invoke(obj);
		yield break;
	}

	public IEnumerator RemovePickableBlock(PickablePonderBlock block)
	{
		m_ponder.PickablePonderBlockSet.Remove(block);
		yield break;
	}

	public IEnumerator RemovePickableBlock(PickablePonderBlockHandle block)
	{
		m_ponder.PickablePonderBlockSet.Remove(block.Instance);
		yield break;
	}

	public IEnumerator AddProjectileBlock(int blockValue, Vector3 position, Vector3 velocity, Vector3 angularVelocity, Action<ProjectilePonderBlock> createdCallback, object tag = null)
	{
		ProjectilePonderBlock obj = m_ponder.ProjectilePonderBlockSet.Add(blockValue, position, velocity, angularVelocity, tag);
		createdCallback?.Invoke(obj);
		yield break;
	}

	public IEnumerator RemoveProjectileBlock(ProjectilePonderBlock block)
	{
		m_ponder.ProjectilePonderBlockSet.Remove(block);
		yield break;
	}

	public IEnumerator RemoveProjectileBlock(ProjectilePonderBlockHandle block)
	{
		m_ponder.ProjectilePonderBlockSet.Remove(block.Instance);
		yield break;
	}
}
