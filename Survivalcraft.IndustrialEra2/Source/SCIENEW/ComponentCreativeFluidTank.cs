using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Drawing.WorldDynamic;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCreativeFluidTank : ComponentTankBase
{
	private int m_fluidValue;

	private Fluid m_fluid;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemDrawingElements m_subsystemDrawingElements;

	public DynamicMeshDrawingElement m_drawingElement;

	public int FluidValue
	{
		get
		{
			return m_fluidValue;
		}
		set
		{
			m_fluidValue = value;
			m_fluid = FluidsManager.GetFluid(value);
			SyncTankSlotFromFluidValue();
			Draw();
		}
	}

	public override void AddTankFluid(int tankIndex, int value, float volume, out float overflow)
	{
		overflow = 0f;
	}

	public override int GetTankValue(int tankIndex)
	{
		if (tankIndex != 0 || m_tanks.Count == 0)
		{
			return 0;
		}
		return m_fluidValue;
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_subsystemDrawingElements = Project.FindSubsystem<SubsystemDrawingElements>(throwOnError: true);
		m_tanks[0].Volume = float.PositiveInfinity;
		FluidValue = valuesDictionary.GetValue("FluidValue", 0);
	}

	private void SyncTankSlotFromFluidValue()
	{
		if (m_tanks.Count != 0)
		{
			m_tanks[0].Value = m_fluidValue;
		}
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("FluidValue", FluidValue);
	}

	public override void OnEntityAdded()
	{
		base.OnEntityAdded();
		m_drawingElement = new DynamicMeshDrawingElement(DrawingMaterial.Opaque(IndustrialModLoader.PureWhiteTexture))
		{
			RenderingMode = ModelRenderingMode.TransparentAfterWater
		};
		m_subsystemDrawingElements.Register(m_drawingElement);
		Draw();
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		m_subsystemDrawingElements.Unregister(m_drawingElement);
		m_drawingElement = null;
	}

	public void Draw()
	{
		if (m_drawingElement != null)
		{
			m_drawingElement.Clear();
			if (m_fluidValue != 0)
			{
				float num = 0.02f;
				float x = 0.98f;
				float num2 = MathUtils.Lerp(num, x, 1f);
				Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates);
				Vector3 vector2 = vector + new Vector3(0.075f, num, 0.075f);
				Vector3 vector3 = vector + new Vector3(0.075f, num, 0.925f);
				Vector3 vector4 = vector + new Vector3(0.925f, num, 0.925f);
				Vector3 vector5 = vector + new Vector3(0.925f, num, 0.075f);
				Vector3 vector6 = vector + new Vector3(0.075f, num2, 0.075f);
				Vector3 vector7 = vector + new Vector3(0.075f, num2, 0.925f);
				Vector3 vector8 = vector + new Vector3(0.925f, num2, 0.925f);
				Vector3 vector9 = vector + new Vector3(0.925f, num2, 0.075f);
				int fluidValue = m_fluidValue;
				float num3 = 1f / (float)m_fluid.GetTextureSubdivision(fluidValue);
				int textureSlot = m_fluid.GetTextureSlot(fluidValue);
				Vector2 vector11;
				Vector2 vector10 = (vector11 = new Vector2(num3 * (float)(textureSlot % m_fluid.GetTextureSubdivision(fluidValue)), num3 * ((float)textureSlot / (float)m_fluid.GetTextureSubdivision(fluidValue))));
				Vector2 vector12 = vector10 + Vector2.UnitX * num3;
				Vector2 vector13 = vector10 + Vector2.One * num3;
				Vector2 vector14 = vector10 + Vector2.UnitY * num3;
				float num4 = (num2 - num) / 0.85f;
				Vector2 texCoord = vector10 + new Vector2(num3, num3 * num4);
				Vector2 texCoord2 = vector10 + new Vector2(0f, num3 * num4);
				m_drawingElement.Material = DrawingMaterial.Opaque(m_fluid.GetTexture(fluidValue)).WithTint(m_fluid.GetColor(fluidValue)).WithUnlit(m_fluid.IsGlow(fluidValue))
					.WithOpacity(1f);
				m_drawingElement.Mesh.QueueQuad(vector2, vector3, vector7, vector6, vector11, vector12, texCoord, texCoord2, Color.White);
				m_drawingElement.Mesh.QueueQuad(vector3, vector4, vector8, vector7, vector11, vector12, texCoord, texCoord2, Color.White);
				m_drawingElement.Mesh.QueueQuad(vector4, vector5, vector9, vector8, vector11, vector12, texCoord, texCoord2, Color.White);
				m_drawingElement.Mesh.QueueQuad(vector5, vector2, vector6, vector9, vector11, vector12, texCoord, texCoord2, Color.White);
				m_drawingElement.Mesh.QueueQuad(vector2, vector3, vector4, vector5, vector11, vector12, vector13, vector14, Color.White);
				m_drawingElement.Mesh.QueueQuad(vector9, vector8, vector7, vector6, vector14, vector13, vector12, vector11, Color.White);
				m_drawingElement.MarkGeometryDirty();
			}
		}
	}
}
