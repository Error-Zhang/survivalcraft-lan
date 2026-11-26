using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace SCIENEW;

public class ComponentCreativeChest : ComponentInventoryBase, IUpdateable
{
	public CreativeChestDevice.Mode m_mode;

	public ComponentBlockEntity m_componentBlockEntity;

	public SubsystemTerrain m_subsystemTerrain;

	public SubsystemPickables m_subsystemPickables;

	public SubsystemProjectiles m_subsystemProjectiles;

	public SubsystemAudio m_subsystemAudio;

	private bool m_dirty;

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public int RefSlotIndex => SlotsCount - 1;

	public override int GetSlotCapacity(int slotIndex, int value)
	{
		if (slotIndex != RefSlotIndex)
		{
			return 0;
		}
		return base.GetSlotCapacity(slotIndex, value);
	}

	public override int RemoveSlotItems(int slotIndex, int count)
	{
		if (slotIndex == RefSlotIndex)
		{
			m_dirty = true;
		}
		if (slotIndex == RefSlotIndex)
		{
			return base.RemoveSlotItems(slotIndex, count);
		}
		return count;
	}

	public override void AddSlotItems(int slotIndex, int value, int count)
	{
		if (slotIndex == RefSlotIndex)
		{
			m_dirty = true;
		}
		base.AddSlotItems(slotIndex, value, count);
	}

	public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
	{
		base.Load(valuesDictionary, idToEntityMap);
		m_mode = valuesDictionary.GetValue("Mode", CreativeChestDevice.Mode.Dispense);
		m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		m_subsystemProjectiles = Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
		m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
	}

	public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
	{
		base.Save(valuesDictionary, entityToIdMap);
		valuesDictionary.SetValue("Mode", m_mode);
	}

	public void Update(float dt)
	{
		if (m_dirty)
		{
			m_dirty = false;
			for (int i = 0; i < RefSlotIndex; i++)
			{
				m_slots[i].Value = GetSlotValue(RefSlotIndex);
				m_slots[i].Count = GetSlotCount(RefSlotIndex);
			}
		}
	}

	public void DispenseOrShoot()
	{
		int direction_Static = SixDirectionModelDevice.GetDirection_Static(m_subsystemTerrain.Terrain.GetCellValueFast(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z));
		Slot slot = m_slots[0];
		if (GetSlotCount(0) == 0)
		{
			return;
		}
		if (m_mode == CreativeChestDevice.Mode.Shoot)
		{
			if (!Shoot(slot, direction_Static))
			{
				Dispense(slot, direction_Static);
			}
		}
		else
		{
			Dispense(slot, direction_Static);
		}
	}

	private void Dispense(Slot slot, int direction)
	{
		Vector3 vector = CellFace.FaceToVector3(direction);
		float num = 1.8f;
		Vector3 position = new Vector3((float)m_componentBlockEntity.Coordinates.X + 0.5f, (float)m_componentBlockEntity.Coordinates.Y + 0.5f, (float)m_componentBlockEntity.Coordinates.Z + 0.5f) + 0.6f * vector;
		m_subsystemPickables.AddPickable(slot.Value, 1, position, num * (vector + m_random.Vector3(0.2f)), null);
		m_subsystemAudio.PlaySound("Audio/DispenserDispense", 1f, 0f, new Vector3(m_componentBlockEntity.Coordinates), 3f, autoDelay: true);
	}

	private bool Shoot(Slot slot, int direction)
	{
		bool flag = m_subsystemProjectiles.FireProjectile(slot.Value, new Vector3(m_componentBlockEntity.Coordinates) + new Vector3(0.5f) + CellFace.FaceToVector3(direction) * 0.6f, CellFace.FaceToVector3(direction) * 40f, Vector3.Zero, null) != null;
		if (flag)
		{
			m_subsystemAudio.PlaySound("Audio/DispenserShoot", 1f, 0f, new Vector3(m_componentBlockEntity.Coordinates), 4f, autoDelay: true);
		}
		return flag;
	}
}
