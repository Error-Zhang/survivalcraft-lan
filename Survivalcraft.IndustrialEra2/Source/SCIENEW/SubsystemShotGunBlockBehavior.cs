using Engine;
using Game;
using SCIENEW.Extensions;

namespace SCIENEW;

public class SubsystemShotGunBlockBehavior : SubsystemMusketBlockBehavior
{
	public SubsystemGameInfo m_subsystemGameInfo;

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
				int data = Terrain.ExtractData(slotValue);
				int num2 = slotValue;
				if (num == ShotGunBlock.Index && slotCount > 0)
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
					direction = (direction + vector).Normalize();
					switch (state)
					{
					case AimState.InProgress:
					{
						if (num3 >= 50f)
						{
							componentMiner.ComponentCreature.ComponentCreatureSounds.PlayMoanSound();
							return true;
						}
						if (num3 > 0.2f && !MusketBlock.GetHammerState(Terrain.ExtractData(num2)))
						{
							num2 = Terrain.MakeBlockValue(num, 0, MusketBlock.SetHammerState(Terrain.ExtractData(num2), state: true));
							m_subsystemAudio.PlaySound("Audio/HammerCock", 1f, m_random.Float(-0.1f, 0.1f), 0f, 0f);
						}
						_ = componentMiner.ComponentPlayer.GameWidget;
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
						componentMiner.ComponentCreature.ComponentCreatureModel.AimHandAngleOrder = 1.4f;
						componentMiner.ComponentCreature.ComponentCreatureModel.InHandItemOffsetOrder = new Vector3(-0.08f, -0.08f, 0.07f);
						componentMiner.ComponentCreature.ComponentCreatureModel.InHandItemRotationOrder = new Vector3(-1.7f, 0f, 0f);
						break;
					}
					case AimState.Cancelled:
						if (MusketBlock.GetHammerState(Terrain.ExtractData(num2)))
						{
							num2 = Terrain.MakeBlockValue(num, 0, MusketBlock.SetHammerState(Terrain.ExtractData(num2), state: false));
							m_subsystemAudio.PlaySound("Audio/HammerUncock", 1f, m_random.Float(-0.1f, 0.1f), 0f, 0f);
						}
						m_aimStartTimes.Remove(componentMiner);
						break;
					case AimState.Completed:
					{
						Vector3 vector2 = new Vector3(0f, 0f, 0f);
						_ = componentMiner.ComponentPlayer.GameWidget;
						bool flag = false;
						int value2 = 0;
						int num5 = 0;
						float num6 = 0f;
						vector2.Normalize();
						Vector3 vector3 = Vector3.Zero;
						MusketBlock.GetLoadState(data);
						if (ShotGunBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(activeSlotIndex))) > 0)
						{
							flag = true;
							value2 = ItemsBlockManager.GetItemID("LeadBullet_ShotGun");
							num6 = 180f;
							num5 = 24;
							vector3 = new Vector3(0.001f, 0.001f, 0.001f);
						}
						if (flag)
						{
							Vector3 eyePosition = componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition;
							Matrix matrix = componentMiner.ComponentCreature.ComponentBody.Matrix;
							Vector3 vector4 = eyePosition + matrix.Right * 0.3f - matrix.Up * 0.2f;
							Vector3 vector5 = (vector4 + direction * 10f - vector4).Normalize();
							Vector3 vector6 = direction.Normalize();
							Vector3 vector7 = vector5.Cross(Vector3.UnitY).Normalize();
							Vector3 vector8 = vector5.Cross(vector7).Normalize();
							for (int i = 0; i < num5; i++)
							{
								Vector3 vector9 = m_random.Float(0f - vector3.X, vector3.X) * vector7 + m_random.Float(0f - vector3.Y, vector3.Y) * vector8 + m_random.Float(0f - vector3.Z, vector3.Z) * vector5;
								Projectile projectile = m_subsystemProjectiles.FireProjectile(value2, vector4, num6 * (vector6 + vector9 * 50f), Vector3.Zero, componentMiner.ComponentCreature);
								if (projectile != null)
								{
									projectile.ProjectileStoppedAction = ProjectileStoppedAction.Disappear;
								}
							}
							m_subsystemAudio.PlaySound("Audio/MusketFire", 1f, m_random.Float(-0.1f, 0.1f), componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition, 10f, autoDelay: true);
							Vector3 vector10 = componentMiner.ComponentPlayer.GameWidget.ActiveCamera.ViewDirection.Normalize();
							m_subsystemParticles.AddParticleSystem(new GunSmokeParticleSystem2(SubsystemTerrain, vector4 + 1.3f * vector10, vector10));
							m_subsystemNoise.MakeNoise(vector4, 1f, 60f);
							int bulletNum = ShotGunBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(activeSlotIndex)));
							componentMiner.ComponentPlayer?.ComponentGui.DisplaySmallMessage((bulletNum - 1).ToString(), Color.White, blinking: true, playNotificationSound: false);
							inventory.RemoveSlotItems(activeSlotIndex, 1);
							inventory.AddSlotItems(activeSlotIndex, Terrain.MakeBlockValue(ShotGunBlock.Index, 0, MathUtils.Max(ShotGunBlock.SetBulletNum(bulletNum - 1), 0)), 1);
						}
						GameWidget gameWidget = componentMiner.ComponentPlayer.GameWidget;
						gameWidget.ActiveCamera = gameWidget.FindCamera<FppCamera>();
						m_aimStartTimes.Remove(componentMiner);
						break;
					}
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
		if (value == ItemsBlockManager.GetItemID("ShotGunBullet") && ShotGunBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(slotIndex))) <= 11)
		{
			return 1;
		}
		return 0;
	}

	public override void ProcessInventoryItem(IInventory inventory, int slotIndex, int value, int count, int processCount, out int processedValue, out int processedCount)
	{
		processedValue = value;
		processedCount = count;
		int bulletNum = ShotGunBlock.GetBulletNum(Terrain.ExtractData(inventory.GetSlotValue(slotIndex)));
		if (processCount + bulletNum < 13 && processCount == 1)
		{
			processedValue = 0;
			processedCount = 0;
			inventory.RemoveSlotItems(slotIndex, 1);
			inventory.AddSlotItems(slotIndex, Terrain.MakeBlockValue(ShotGunBlock.Index, 0, ShotGunBlock.SetBulletNum(MathUtils.Min(bulletNum + processCount, 12))), 1);
		}
	}
}
