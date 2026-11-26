using Engine;
using Game;
using GameEntitySystem;
using SCIENEW.Extensions;
using TemplatesDatabase;

namespace SCIENEW;

public class SubsystemAutoRifleBlockBehavior : SubsystemMusketBlockBehavior
{
	public SubsystemPickables m_subsystemPickables;

	public override void Load(ValuesDictionary valuesDictionary)
	{
		m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
		base.Load(valuesDictionary);
	}

	public override bool OnAim(Ray3 aim, ComponentMiner componentMiner, AimState state)
	{
		Vector3 direction = aim.Direction;
		IInventory inventory = componentMiner.Inventory;
		if (inventory != null)
		{
			int activeSlotIndex = inventory.ActiveSlotIndex;
			if (activeSlotIndex >= 0)
			{
				int slotValue = inventory.GetSlotValue(activeSlotIndex);
				int slotCount = inventory.GetSlotCount(activeSlotIndex);
				int num = Terrain.ExtractContents(slotValue);
				int num2 = slotValue;
				if (num == AutoRifleBlock.Index && slotCount > 0)
				{
					if (!m_aimStartTimes.TryGetValue(componentMiner, out var value))
					{
						value = m_subsystemTime.GameTime;
						m_aimStartTimes[componentMiner] = value;
					}
					float num3 = (float)(m_subsystemTime.GameTime - value);
					float num4 = (float)MathUtils.Remainder(m_subsystemTime.GameTime, 1000.0);
					Vector3 vector = (float)((componentMiner.ComponentCreature.ComponentBody.IsSneaking ? 0.00999999977648258 : 0.0299999993294477) + 0.200000002980232 * (double)MathUtils.Saturate((num3 - 6.5f) / 40f)) * new Vector3
					{
						X = SimplexNoise.OctavedNoise(num4, 2f, 3, 2f, 0.5f),
						Y = SimplexNoise.OctavedNoise(num4 + 100f, 2f, 3, 2f, 0.5f),
						Z = SimplexNoise.OctavedNoise(num4 + 200f, 2f, 3, 2f, 0.5f)
					};
					if (num3 > 1f)
					{
						if (0.2f * num3 < 1.2f)
						{
							direction.Y += 0.1f * (num3 - 1f);
						}
						else
						{
							direction.Y += 0.3f;
						}
					}
					direction = (direction + vector * 2f).Normalize();
					switch (state)
					{
					case AimState.InProgress:
					{
						if (num3 > 0.3f && !RifleBlock.GetHammerState(Terrain.ExtractData(num2)))
						{
							num2 = Terrain.MakeBlockValue(num, 0, RifleBlock.SetHammerState(Terrain.ExtractData(num2), state: true));
							m_subsystemAudio.PlaySound("Audio/HammerCock", 1f, m_random.Float(-0.1f, 0.1f), 0f, 0f);
						}
						ComponentFirstPersonModel componentFirstPersonModel = componentMiner.Entity.FindComponent<ComponentFirstPersonModel>();
						if (componentFirstPersonModel != null)
						{
							ComponentPlayer componentPlayer = componentMiner.ComponentPlayer;
							if (componentPlayer != null)
							{
								componentPlayer?.ComponentAimingSights.ShowAimingSights(aim.Position, direction);
							}
							componentFirstPersonModel.ItemOffsetOrder = new Vector3(-0.21f, 0.15f, 0.08f);
							componentFirstPersonModel.ItemRotationOrder = new Vector3(-0.7f, 0f, 0f);
						}
						if (m_subsystemTime.PeriodicGameTimeEvent(0.17, 0.0) && AutoRifleBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(activeSlotIndex))) > 0)
						{
							Vector3 vector2 = componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition + componentMiner.ComponentCreature.ComponentBody.Matrix.Right * 0.3f - componentMiner.ComponentCreature.ComponentBody.Matrix.Up * 0.2f;
							Vector3 vector3 = (vector2 + direction * 10f - vector2).Normalize();
							Vector3 vector4 = vector3.Cross(Vector3.UnitY).Normalize();
							Vector3 vector5 = vector3.Cross(vector4).Normalize();
							int num5 = 1;
							Vector3 vector6 = new Vector3(0.01f, 0.01f, 0.05f);
							for (int i = 0; i < num5; i++)
							{
								int itemID = ItemsBlockManager.GetItemID("LeadBullet");
								Vector3 vector7 = m_random.Float(0f - vector6.X, vector6.X) * vector4 + m_random.Float(0f - vector6.Y, vector6.Y) * vector5 + m_random.Float(0f - vector6.Z, vector6.Z) * vector3;
								Projectile projectile = m_subsystemProjectiles.FireProjectile(itemID, vector2, 280f * (vector3 + vector7), Vector3.Zero, componentMiner.ComponentCreature);
								if (projectile != null)
								{
									projectile.ProjectileStoppedAction = ProjectileStoppedAction.Disappear;
								}
							}
							m_subsystemAudio.PlaySound("Audio/MusketFire", 1f, m_random.Float(-0.1f, 0.1f), componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition, 10f, autoDelay: true);
							Vector3 vector8 = componentMiner.ComponentPlayer.GameWidget.ActiveCamera.ViewDirection.Normalize();
							m_subsystemParticles.AddParticleSystem(new GunSmokeParticleSystem2(SubsystemTerrain, vector2 + 1.3f * vector8, vector8));
							m_subsystemNoise.MakeNoise(vector2, 1f, 40f);
							int bulletNum = AutoRifleBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(activeSlotIndex)));
							componentMiner.ComponentPlayer?.ComponentGui.DisplaySmallMessage((bulletNum - 1).ToString(), Color.White, blinking: true, playNotificationSound: false);
							inventory.RemoveSlotItems(activeSlotIndex, 1);
							inventory.AddSlotItems(activeSlotIndex, Terrain.MakeBlockValue(AutoRifleBlock.Index, 0, AutoRifleBlock.SetBulletNum(bulletNum - 1)), 1);
						}
						componentMiner.ComponentCreature.ComponentCreatureModel.AimHandAngleOrder = 1.4f;
						componentMiner.ComponentCreature.ComponentCreatureModel.InHandItemOffsetOrder = new Vector3(-0.08f, -0.08f, 0.07f);
						componentMiner.ComponentCreature.ComponentCreatureModel.InHandItemRotationOrder = new Vector3(-1.7f, 0f, 0f);
						break;
					}
					case AimState.Cancelled:
						if (RifleBlock.GetHammerState(Terrain.ExtractData(num2)))
						{
							num2 = Terrain.MakeBlockValue(num, 0, RifleBlock.SetHammerState(Terrain.ExtractData(num2), state: false));
							m_subsystemAudio.PlaySound("Audio/HammerUncock", 1f, m_random.Float(-0.1f, 0.1f), 0f, 0f);
						}
						m_aimStartTimes.Remove(componentMiner);
						break;
					case AimState.Completed:
						if (RifleBlock.GetHammerState(Terrain.ExtractData(num2)))
						{
							num2 = Terrain.MakeBlockValue(num, 0, RifleBlock.SetHammerState(Terrain.ExtractData(num2), state: false));
							m_subsystemAudio.PlaySound("Audio/HammerUncock", 1f, m_random.Float(-0.1f, 0.1f), 0f, 0f);
						}
						m_aimStartTimes.Remove(componentMiner);
						break;
					}
				}
				if (num2 != slotValue)
				{
					inventory.RemoveSlotItems(activeSlotIndex, 1);
					inventory.AddSlotItems(activeSlotIndex, num2, 1);
				}
			}
		}
		return false;
	}

	public override int GetProcessInventoryItemCapacity(IInventory inventory, int slotIndex, int value)
	{
		for (int i = 0; i <= 63; i++)
		{
			if (value == ItemsBlockManager.GetItemID("Clip." + i))
			{
				return 1;
			}
		}
		if (value == ItemsBlockManager.GetItemID("StandardBullet") && AutoRifleBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(slotIndex))) <= 62)
		{
			return 1;
		}
		return 0;
	}

	public override void ProcessInventoryItem(IInventory inventory, int slotIndex, int value, int count, int processCount, out int processedValue, out int processedCount)
	{
		processedValue = value;
		processedCount = count;
		if (value == ItemsBlockManager.GetItemID("StandardBullet"))
		{
			int bulletNum = AutoRifleBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(slotIndex)));
			if (processCount + bulletNum < 64)
			{
				processedValue = 0;
				processedCount = 0;
				inventory.RemoveSlotItems(slotIndex, 1);
				inventory.AddSlotItems(slotIndex, Terrain.MakeBlockValue(AutoRifleBlock.Index, 0, AutoRifleBlock.SetBulletNum(bulletNum + processCount)), 1);
			}
		}
		for (int i = 0; i <= 63; i++)
		{
			if (value != ItemsBlockManager.GetItemID("Clip." + i))
			{
				continue;
			}
			int bulletNum2 = AutoRifleBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(slotIndex)));
			processedValue = 0;
			processedCount = 0;
			inventory.RemoveSlotItems(slotIndex, 1);
			inventory.AddSlotItems(slotIndex, Terrain.MakeBlockValue(AutoRifleBlock.Index, 0, AutoRifleBlock.SetBulletNum(i)), 1);
			foreach (Entity entity in inventory.Project.Entities)
			{
				if (entity != null && entity.FindComponent<ComponentPlayer>() != null)
				{
					m_subsystemPickables.AddPickable(ItemsBlockManager.GetItemID("Clip." + bulletNum2), 1, entity.FindComponent<ComponentBody>().Position, null, null);
					break;
				}
			}
		}
	}
}
