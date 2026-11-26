using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Drawing.WorldDynamic;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentFluidTank : ComponentTankBase, IUpdateable
{
	public static float UnitVolume = 64f;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemDrawingElements m_subsystemDrawingElements;

	public ComponentBlockEntity m_componentBlockEntity;

	public Fluid m_containsFluid;

	public DynamicMeshDrawingElement m_drawingElement;

	public int m_lastFluidID = -1;

	private bool m_drawRequire = true;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public float Capacity
	{
		get
		{
			return m_tanks[0].Capacity;
		}
		set
		{
			m_tanks[0].Capacity = value;
		}
	}

	public bool IsCapacityResolved { get; private set; }

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		m_subsystemDrawingElements = Project.FindSubsystem<SubsystemDrawingElements>(throwOnError: true);
		float value = valuesDictionary.GetValue("Capacity", 0f);
		if (value > 0f)
		{
			Capacity = value;
			IsCapacityResolved = true;
		}
		else
		{
			Capacity = 0f;
			IsCapacityResolved = false;
		}
		m_drawRequire = true;
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		if (IsCapacityResolved && Capacity > 0f)
		{
			valuesDictionary.SetValue("Capacity", Capacity);
		}
	}

	internal void ApplyLanState(int value, float volume, float capacity)
	{
		Tank tank = m_tanks[0];
		if (tank.Value != value || tank.Volume != volume || tank.Capacity != capacity)
		{
			tank.Value = value;
			tank.Volume = volume;
			Capacity = capacity;
			IsCapacityResolved = capacity > 0f;
			m_drawRequire = true;
		}
	}

	public void Update(float dt)
	{
		bool isRemote = IndustrialEraLanAdapter.IsRemote(Project);
		if (!isRemote)
		{
			TryResolveCapacity(isFast: false);
		}
		if (!isRemote && IsCapacityResolved && m_tanks[0].Volume > Capacity)
		{
			m_tanks[0].Volume = Capacity;
			m_containsFluid?.Emit(Project, new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f), Vector3.Zero);
			m_drawRequire = true;
		}
		if (m_tanks[0].Value != m_lastFluidID)
		{
			m_lastFluidID = m_tanks[0].Value;
			OnSwitchFluid();
		}
		if (!m_drawRequire)
		{
			return;
		}
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z);
		bool flag;
		if (chunkAtCell != null)
		{
			TerrainChunkState state = chunkAtCell.State;
			if ((uint)(state - 5) <= 1u || state == TerrainChunkState.Valid)
			{
				flag = true;
				goto IL_0104;
			}
		}
		flag = false;
		goto IL_0104;
		IL_0104:
		if (flag)
		{
			Draw();
			m_drawRequire = false;
		}
	}

	public override void AddTankFluid(int tankIndex, int value, float volume, out float overflow)
	{
		if (!IsCapacityResolved || Capacity <= 0f)
		{
			overflow = 0f;
			if (volume <= 0f || tankIndex != 0 || m_tanks.Count == 0)
			{
				return;
			}
			Tank tank = m_tanks[0];
			if (tank.Volume > 0f && tank.Value != value && value != 0)
			{
				overflow = volume;
				return;
			}
			if (value != 0)
			{
				tank.Value = value;
			}
			tank.Volume += volume;
			m_drawRequire = true;
		}
		else
		{
			base.AddTankFluid(tankIndex, value, volume, out overflow);
			m_drawRequire = true;
		}
	}

	public override float RemoveTankFluid(int tankIndex, float volume)
	{
		m_drawRequire = true;
		return base.RemoveTankFluid(tankIndex, volume);
	}

	public void OnSwitchFluid()
	{
		m_containsFluid = FluidsManager.GetFluid(m_tanks[0].Value);
		if (m_containsFluid != null && m_drawingElement != null)
		{
			int value = m_tanks[0].Value;
			m_drawingElement.Material = DrawingMaterial.Opaque(m_containsFluid.GetTexture(value)).WithTint(m_containsFluid.GetColor(value)).WithUnlit(m_containsFluid.IsGlow(value));
		}
	}

	public void UpdateVolume(bool isFast = true)
	{
		TryResolveCapacity(isFast);
	}

	public bool TryResolveCapacity(bool isFast = true)
	{
		Point3 coordinates = m_componentBlockEntity.Coordinates;
		if (m_subsystemTerrain.Terrain.GetChunkAtCell(coordinates.X, coordinates.Z) == null)
		{
			return false;
		}
		if (!IsTank(coordinates, isFast))
		{
			return false;
		}
		int num = 0;
		Point3 point = coordinates;
		while (IsTank(point, isFast))
		{
			point += Point3.UnitY;
			num++;
		}
		Capacity = UnitVolume * (float)num;
		IsCapacityResolved = num > 0;
		return IsCapacityResolved;
	}

	public override void OnEntityAdded()
	{
		base.OnEntityAdded();
		m_drawingElement = new DynamicMeshDrawingElement(DrawingMaterial.Opaque(IndustrialModLoader.PureWhiteTexture))
		{
			RenderingMode = ModelRenderingMode.TransparentAfterWater
		};
		m_subsystemDrawingElements.Register(m_drawingElement);
	}

	public override void OnEntityRemoved()
	{
		base.OnEntityRemoved();
		m_subsystemDrawingElements.Unregister(m_drawingElement);
	}

	private bool IsTank(Point3 point, bool isFast = true)
	{
		int value = (isFast ? m_subsystemTerrain.Terrain.GetCellValueFast(point.X, point.Y, point.Z) : m_subsystemTerrain.Terrain.GetCellValue(point.X, point.Y, point.Z));
		if (Terrain.ExtractContents(value) == BaseDeviceBlock.Index && DevicesBlockManager.GetDevice(value) is FluidTankDevice)
		{
			return true;
		}
		return false;
	}

	public void Draw()
	{
		m_drawingElement.Clear();
		if (m_containsFluid != null && m_tanks[0].Volume > 0f)
		{
			if (m_containsFluid.GetFluidType(GetTankValue(0)) == FluidType.Liquid)
			{
				float num = 0.02f;
				float x = (Capacity - UnitVolume) / UnitVolume * 1f + 0.98f;
				float num2 = MathUtils.Lerp(num, x, m_tanks[0].Volume / Capacity);
				Vector3 vector = new Vector3(m_componentBlockEntity.Coordinates);
				Vector3 vector2 = vector + new Vector3(0.075f, num, 0.075f);
				Vector3 vector3 = vector + new Vector3(0.075f, num, 0.925f);
				Vector3 vector4 = vector + new Vector3(0.925f, num, 0.925f);
				Vector3 vector5 = vector + new Vector3(0.925f, num, 0.075f);
				Vector3 vector6 = vector + new Vector3(0.075f, num2, 0.075f);
				Vector3 vector7 = vector + new Vector3(0.075f, num2, 0.925f);
				Vector3 vector8 = vector + new Vector3(0.925f, num2, 0.925f);
				Vector3 vector9 = vector + new Vector3(0.925f, num2, 0.075f);
				int value = m_tanks[0].Value;
				float num3 = 1f / (float)m_containsFluid.GetTextureSubdivision(value);
				int textureSlot = m_containsFluid.GetTextureSlot(value);
				Vector2 vector11;
				Vector2 vector10 = (vector11 = new Vector2(num3 * (float)(textureSlot % m_containsFluid.GetTextureSubdivision(value)), num3 * (float)(textureSlot / m_containsFluid.GetTextureSubdivision(value))));
				Vector2 vector12 = vector10 + Vector2.UnitX * num3;
				Vector2 vector13 = vector10 + Vector2.One * num3;
				Vector2 vector14 = vector10 + Vector2.UnitY * num3;
				float num4 = (num2 - num) / 0.85f;
				Vector2 texCoord = vector10 + new Vector2(num3, num3 * num4);
				Vector2 texCoord2 = vector10 + new Vector2(0f, num3 * num4);
				m_drawingElement.Material = DrawingMaterial.Opaque(m_containsFluid.GetTexture(value)).WithTint(m_containsFluid.GetColor(value)).WithUnlit(m_containsFluid.IsGlow(value))
					.WithOpacity(1f);
				(vector3 - vector2).Cross(vector6 - vector2).Normalize();
				m_drawingElement.Mesh.QueueQuad(vector2, vector3, vector7, vector6, vector11, vector12, texCoord, texCoord2, Color.White);
				(vector4 - vector3).Cross(vector7 - vector3).Normalize();
				m_drawingElement.Mesh.QueueQuad(vector3, vector4, vector8, vector7, vector11, vector12, texCoord, texCoord2, Color.White);
				(vector5 - vector4).Cross(vector8 - vector4).Normalize();
				m_drawingElement.Mesh.QueueQuad(vector4, vector5, vector9, vector8, vector11, vector12, texCoord, texCoord2, Color.White);
				(vector2 - vector5).Cross(vector9 - vector5).Normalize();
				m_drawingElement.Mesh.QueueQuad(vector5, vector2, vector6, vector9, vector11, vector12, texCoord, texCoord2, Color.White);
				m_drawingElement.Mesh.QueueQuad(vector2, vector3, vector4, vector5, vector11, vector12, vector13, vector14, Color.White);
				m_drawingElement.Mesh.QueueQuad(vector9, vector8, vector7, vector6, vector14, vector13, vector12, vector11, Color.White);
			}
			else if (m_containsFluid.GetFluidType(GetTankValue(0)) == FluidType.Gas)
			{
				float num5 = 0.02f;
				float num6 = (Capacity - UnitVolume) / UnitVolume * 1f + 0.98f;
				Vector3 vector15 = new Vector3(m_componentBlockEntity.Coordinates);
				Vector3 vector16 = vector15 + new Vector3(0.075f, num5, 0.075f);
				Vector3 vector17 = vector15 + new Vector3(0.075f, num5, 0.925f);
				Vector3 vector18 = vector15 + new Vector3(0.925f, num5, 0.925f);
				Vector3 vector19 = vector15 + new Vector3(0.925f, num5, 0.075f);
				Vector3 vector20 = vector15 + new Vector3(0.075f, num6, 0.075f);
				Vector3 vector21 = vector15 + new Vector3(0.075f, num6, 0.925f);
				Vector3 vector22 = vector15 + new Vector3(0.925f, num6, 0.925f);
				Vector3 vector23 = vector15 + new Vector3(0.925f, num6, 0.075f);
				int value2 = m_tanks[0].Value;
				float num7 = 1f / (float)m_containsFluid.GetTextureSubdivision(value2);
				int textureSlot2 = m_containsFluid.GetTextureSlot(value2);
				Vector2 vector25;
				Vector2 vector24 = (vector25 = new Vector2(num7 * (float)(textureSlot2 % m_containsFluid.GetTextureSubdivision(value2)), num7 * (float)(textureSlot2 / m_containsFluid.GetTextureSubdivision(value2))));
				Vector2 vector26 = vector24 + Vector2.UnitX * num7;
				Vector2 vector27 = vector24 + Vector2.One * num7;
				Vector2 vector28 = vector24 + Vector2.UnitY * num7;
				float num8 = (num6 - num5) / 0.85f;
				Vector2 texCoord3 = vector24 + new Vector2(num7, num7 * num8);
				Vector2 texCoord4 = vector24 + new Vector2(0f, num7 * num8);
				m_containsFluid.GetColor(value2);
				float f = ((Capacity <= 0f) ? 0f : MathUtils.Saturate(m_tanks[0].Volume / Capacity));
				m_drawingElement.Material = DrawingMaterial.Opaque(m_containsFluid.GetTexture(value2)).WithTint(m_containsFluid.GetColor(value2)).WithUnlit(m_containsFluid.IsGlow(value2))
					.WithOpacity(MathUtils.Lerp(0.15f, 0.65f, f));
				(vector17 - vector16).Cross(vector20 - vector16).Normalize();
				m_drawingElement.Mesh.QueueQuad(vector16, vector17, vector21, vector20, vector25, vector26, texCoord3, texCoord4, Color.White);
				(vector18 - vector17).Cross(vector21 - vector17).Normalize();
				m_drawingElement.Mesh.QueueQuad(vector17, vector18, vector22, vector21, vector25, vector26, texCoord3, texCoord4, Color.White);
				Vector3.Normalize((vector19 - vector18).Cross(vector22 - vector18));
				m_drawingElement.Mesh.QueueQuad(vector18, vector19, vector23, vector22, vector25, vector26, texCoord3, texCoord4, Color.White);
				Vector3.Normalize((vector16 - vector19).Cross(vector23 - vector19));
				m_drawingElement.Mesh.QueueQuad(vector19, vector16, vector20, vector23, vector25, vector26, texCoord3, texCoord4, Color.White);
				m_drawingElement.Mesh.QueueQuad(vector16, vector17, vector18, vector19, vector25, vector26, vector27, vector28, Color.White);
				m_drawingElement.Mesh.QueueQuad(vector23, vector22, vector21, vector20, vector28, vector27, vector26, vector25, Color.White);
			}
			m_drawingElement.MarkGeometryDirty();
		}
	}
}
