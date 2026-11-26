using Engine;
using Engine.Graphics;
using Game;
using GameEntitySystem;
using SCIENEW.Drawing.WorldDynamic;
using SCIENEW.VoltNet;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentAmmeter : Component, IUpdateable
{
	public ComponentBlockEntity m_componentBlockEntity;

	private SubsystemTerrain m_subsystemTerrain;

	private SubsystemVoltNet m_subsystemVoltNet;

	private SubsystemDrawingElements m_subsystemDrawingElements;

	private BlockGeometryDrawingElement m_drawingElement;

	private float m_displayPointerAngle;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float Range { get; set; } = 16f;

	public float Current { get; private set; }

	public float DisplayCurrent { get; private set; }

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemVoltNet = Project.FindSubsystem<SubsystemVoltNet>(throwOnError: true);
		m_subsystemDrawingElements = Project.FindSubsystem<SubsystemDrawingElements>(throwOnError: true);
		Range = MathUtils.Clamp(valuesDictionary.GetValue("Range", 16f), 4f, 64f);
		DisplayCurrent = valuesDictionary.GetValue("DisplayCurrent", 0f);
		m_displayPointerAngle = AmmeterDevice.CalcPointerAngle(DisplayCurrent, Range);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		valuesDictionary.SetValue("Range", Range);
		valuesDictionary.SetValue("DisplayCurrent", DisplayCurrent);
	}

	public override void OnEntityAdded()
	{
		base.OnEntityAdded();
		m_drawingElement = new BlockGeometryDrawingElement(() => DrawingMaterial.Opaque(IndustrialModLoader.IETexture).WithVertexNormals(useVertexNormals: true), (PrimitivesRenderer3D primitives, DrawBlockEnvironmentData environment) =>
		{
			AmmeterDevice device = DevicesBlockManager.GetDevice<AmmeterDevice>();
			if (device != null)
			{
				Point3 coordinates = m_componentBlockEntity.Coordinates;
				int direction_Static = AmmeterDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValueFast(coordinates.X, coordinates.Y, coordinates.Z));
				Matrix matrix = device.CalcPointerMatrix(coordinates, direction_Static, m_displayPointerAngle);
				BlocksManager.DrawMeshBlock(primitives, device.m_pointerMesh, IndustrialModLoader.IETexture, Color.White, 1f, ref matrix, environment);
			}
		})
		{
			RenderingMode = ModelRenderingMode.AlphaThreshold
		};
		m_subsystemDrawingElements.Register(m_drawingElement);
		RedrawPointer();
	}

	public override void OnEntityRemoved()
	{
		if (m_drawingElement != null)
		{
			m_subsystemDrawingElements.Unregister(m_drawingElement);
			m_drawingElement = null;
		}
		base.OnEntityRemoved();
	}

	public void Update(float dt)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		Current = m_subsystemVoltNet.GetWireGroupCurrent(coordinates);
		DisplayCurrent = MathUtils.Lerp(DisplayCurrent, Current, MathUtils.Saturate(8f * dt));
		float x = AmmeterDevice.CalcPointerAngle(DisplayCurrent, Range);
		m_displayPointerAngle = MathUtils.Lerp(m_displayPointerAngle, x, MathUtils.Saturate(10f * dt));
		RedrawPointer();
	}

	private void RedrawPointer()
	{
		if (m_drawingElement != null)
		{
			m_drawingElement.Rebuild(m_subsystemTerrain, m_componentBlockEntity.Coordinates);
		}
	}
}
