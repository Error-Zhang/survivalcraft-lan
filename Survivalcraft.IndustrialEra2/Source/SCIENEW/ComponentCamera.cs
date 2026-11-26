using System;
using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Screens;
using SCIENEW.Utils;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCamera : Component, IUpdateable, IScreenProviderComponent
{
	public const int MaxPitch = 60;

	public const int MaxYaw = 180;

	public const int MinPitch = -60;

	public const int MinYaw = -180;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemVoltNet m_subsystemVoltNet;

	public SubsystemCameraGameWidgets m_subsystemCameraGameWidgets;

	public PerspectiveCamera m_camera;

	public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

	public ComponentBlockEntity m_componentBlockEntity;

	public ComponentPerspectiveCameraMaskModel m_componentIEModel;

	public GameWidget m_gameWidget;

	public bool m_removed;

	public bool m_drawViewRequire;

	public WorkState m_workState;

	public int[] DrawOrders => new int[2] { 202, 1101 };

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int Yaw { get; set; }

	public int Pitch { get; set; }

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_subsystemCameraGameWidgets = Project.FindSubsystem<SubsystemCameraGameWidgets>(throwOnError: true);
		m_componentIEModel = Entity.FindComponent<ComponentPerspectiveCameraMaskModel>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		Yaw = valuesDictionary.GetValue("Yaw", 0);
		Pitch = valuesDictionary.GetValue("Pitch", 0);
		CameraDevice device = DevicesBlockManager.GetDevice<CameraDevice>();
		if (device != null)
		{
			m_componentIEModel.SetModel(device.m_model);
			m_componentIEModel.SetTexture(device.m_texture);
			m_componentIEModel.MainBoneName = m_componentIEModel.Model.RootBone.Name;
		}
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("Yaw", Yaw);
		valuesDictionary.SetValue("Pitch", Pitch);
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		if (m_camera != null || m_gameWidget != null)
		{
			m_subsystemCameraGameWidgets.RemoveGameWidget(m_gameWidget, m_camera);
			m_gameWidget = null;
			m_camera = null;
			m_removed = true;
		}
	}

	public void Update(float dt)
	{
		if (m_removed)
		{
			return;
		}
		UpdateDrawing();
		if (m_componentBlockEntity != null)
		{
			Point3 coordinates = m_componentBlockEntity.Coordinates;
			VoltElement voltElement = m_subsystemVoltNet.GetVoltElement(coordinates);
			if (voltElement == null)
			{
				m_workState = WorkState.Broken;
				return;
			}
			m_workState = voltElement.CurrentWorkState;
		}
		if (m_workState != WorkState.Active)
		{
			m_drawViewRequire = false;
			return;
		}
		if (m_camera == null)
		{
			m_gameWidget = m_subsystemCameraGameWidgets.AddGameWidget(CreateViewMatrix(), CreateProjectionMatrix(), out m_camera);
			m_componentIEModel.MaskCamera = m_camera;
		}
		m_camera.SetViewMatrix(CreateViewMatrix());
		if (m_drawViewRequire)
		{
			m_drawViewRequire = false;
			if (m_subsystemTerrain.TerrainRenderer.m_chunksToDraw.Count > 0)
			{
				m_subsystemCameraGameWidgets.DrawView(m_camera, setUpdateLocation: false);
			}
		}
	}

	public void UpdateDrawing()
	{
		Matrix value = Matrix.CreateTranslation(GetPosition() + new Vector3(0f, -0.5f, 0f));
		Matrix value2 = Matrix.CreateFromYawPitchRoll(MathUtils.DegToRad(Yaw), MathUtils.DegToRad(Pitch), 0f);
		m_componentIEModel.SetBoneTransform(m_componentIEModel.Model.RootBone.Index, value);
		m_componentIEModel.SetBoneTransform("Camera", value2);
	}

	public Matrix CreateViewMatrix()
	{
		Matrix matrix = Matrix.CreateTranslation(new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f));
		return Matrix.CreateFromYawPitchRoll(MathUtils.DegToRad(Yaw), MathUtils.DegToRad(Pitch), 0f) * matrix;
	}

	public Matrix CreateProjectionMatrix()
	{
		float num = 80f;
		float num2 = 1f;
		float num3 = MathF.Min(num * num2, num);
		float num4 = num3 * num2;
		if (num4 < 90f)
		{
			num3 *= 90f / num4;
		}
		else if (num4 > 175f)
		{
			num3 *= 175f / num4;
		}
		return Matrix.CreatePerspectiveFieldOfView(MathUtils.DegToRad(num3), num2, 0.1f, 2048f);
	}

	public void Draw(SCIENEW.Screens.Screen screen, Project project, Camera camera, int drawOrder)
	{
		if (m_gameWidget != null && m_camera != null && m_workState == WorkState.Active)
		{
			m_drawViewRequire = true;
			ScreensManager.QueueQuad(m_primitivesRenderer.TexturedBatch(m_camera.ViewTexture, useAlphaTest: true, 0, null, SCIENEW.Screens.Screen.QuadRasterizerState, null, SamplerState.PointClamp), screen.Corner, screen.Right, screen.Up, Color.White);
			m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
		}
	}

	public Entity GetEntity()
	{
		return Entity;
	}

	public Vector3 GetPosition()
	{
		return new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f);
	}

	public BoundingBox GetBoundingBox()
	{
		return new BoundingBox(new Vector3(m_componentBlockEntity.Coordinates), new Vector3(m_componentBlockEntity.Coordinates) + Vector3.One);
	}

	public string GetDescription()
	{
		return LanguageUtils.GetText(this, "ProviderDescription");
	}
}
