using Engine;
using Game;
using SCIENEW.Extensions;

namespace SCIENEW;

public class SubsystemRifleBlockBehavior : SubsystemMusketBlockBehavior
{
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
				int num3 = 0;
				if (num == RifleBlock.Index && slotCount > 0)
				{
					if (!m_aimStartTimes.TryGetValue(componentMiner, out var value))
					{
						value = m_subsystemTime.GameTime;
						m_aimStartTimes[componentMiner] = value;
					}
					float num4 = (float)(m_subsystemTime.GameTime - value);
					float num5 = (float)MathUtils.Remainder(m_subsystemTime.GameTime, 1000.0);
					Vector3 vector = (float)((componentMiner.ComponentCreature.ComponentBody.IsSneaking ? 0.00999999977648258 : 0.0299999993294477) + 0.200000002980232 * (double)MathUtils.Saturate((num4 - 6.5f) / 40f)) * new Vector3
					{
						X = SimplexNoise.OctavedNoise(num5, 2f, 3, 2f, 0.5f),
						Y = SimplexNoise.OctavedNoise(num5 + 100f, 2f, 3, 2f, 0.5f),
						Z = SimplexNoise.OctavedNoise(num5 + 200f, 2f, 3, 2f, 0.5f)
					};
					direction = (direction + vector).Normalize();
					switch (state)
					{
					case AimState.InProgress:
					{
						if (num4 >= 30f)
						{
							componentMiner.ComponentCreature.ComponentCreatureSounds.PlayMoanSound();
							return true;
						}
						if (num4 > 0.2f && !RifleBlock.GetHammerState(Terrain.ExtractData(num2)))
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
					{
						bool flag = false;
						int value2 = 0;
						int num6 = 0;
						float num7 = 0f;
						Vector3 vector2 = Vector3.Zero;
						RifleBlock.LoadState loadState = RifleBlock.GetLoadState(data);
						if (RifleBlock.GetHammerState(Terrain.ExtractData(num2)))
						{
							if (loadState != RifleBlock.LoadState.Empty)
							{
								if (loadState == RifleBlock.LoadState.Bullet)
								{
									flag = true;
									value2 = ItemsBlockManager.GetItemID("IronBullet");
									num7 = 320f;
									num6 = 1;
									vector2 = new Vector3(0.01f, 0.01f, 0.05f);
								}
								if (loadState == RifleBlock.LoadState.Bullet2)
								{
									flag = true;
									value2 = ItemsBlockManager.GetItemID("IronBullet");
									num7 = 320f;
									num6 = 1;
									vector2 = new Vector3(0.01f, 0.01f, 0.05f);
								}
							}
							else
							{
								componentMiner.ComponentPlayer?.ComponentGui.DisplaySmallMessage("Load bullet first", Color.White, blinking: true, playNotificationSound: false);
							}
						}
						if (flag)
						{
							if (componentMiner.ComponentCreature.ComponentBody.ImmersionFactor > 0.4f)
							{
								m_subsystemAudio.PlaySound("Audio/MusketMisfire", 1f, m_random.Float(-0.1f, 0.1f), componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition, 3f, autoDelay: true);
							}
							else
							{
								Vector3 eyePosition = componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition;
								Matrix matrix = componentMiner.ComponentCreature.ComponentBody.Matrix;
								Vector3 vector3 = eyePosition + matrix.Right * 0.3f - matrix.Up * 0.2f;
								Vector3 vector4 = (vector3 + aim.Direction * 10f - vector3).Normalize();
								Vector3 vector5 = vector4.Cross(Vector3.UnitY).Normalize();
								Vector3 vector6 = vector4.Cross(vector5).Normalize();
								for (int i = 0; i < num6; i++)
								{
									Vector3 vector7 = m_random.Float(0f - vector2.X, vector2.X) * vector5 + m_random.Float(0f - vector2.Y, vector2.Y) * vector6 + m_random.Float(0f - vector2.Z, vector2.Z) * vector4;
									Projectile projectile = m_subsystemProjectiles.FireProjectile(value2, vector3 + 0.5f * (vector4 + vector7), num7 * (vector4 + vector7), Vector3.Zero, componentMiner.ComponentCreature);
									if (projectile != null)
									{
										projectile.ProjectileStoppedAction = ProjectileStoppedAction.Disappear;
									}
								}
								m_subsystemAudio.PlaySound("Audio/MusketFire", 1f, m_random.Float(-0.1f, 0.1f), componentMiner.ComponentCreature.ComponentCreatureModel.EyePosition, 10f, autoDelay: true);
								m_subsystemParticles.AddParticleSystem(new GunSmokeParticleSystem(SubsystemTerrain, vector3 + 0.3f * vector4, vector4));
								m_subsystemNoise.MakeNoise(vector3, 1f, 40f);
								componentMiner.ComponentCreature.ComponentBody.ApplyImpulse(-1f * vector4);
							}
							if (loadState == RifleBlock.LoadState.Bullet)
							{
								num2 = Terrain.MakeBlockValue(Terrain.ExtractContents(num2), 0, RifleBlock.SetLoadState(Terrain.ExtractData(num2), RifleBlock.LoadState.Empty));
							}
							if (loadState == RifleBlock.LoadState.Bullet2)
							{
								num2 = Terrain.MakeBlockValue(Terrain.ExtractContents(num2), 0, RifleBlock.SetLoadState(Terrain.ExtractData(num2), RifleBlock.LoadState.Bullet));
							}
							num3 = 1;
						}
						if (RifleBlock.GetHammerState(Terrain.ExtractData(num2)))
						{
							num2 = Terrain.MakeBlockValue(Terrain.ExtractContents(num2), 0, RifleBlock.SetHammerState(Terrain.ExtractData(num2), state: false));
							m_subsystemAudio.PlaySound("Audio/HammerRelease", 1f, m_random.Float(-0.1f, 0.1f), 0f, 0f);
						}
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
				if (num3 > 0)
				{
					componentMiner.DamageActiveTool(num3);
				}
			}
		}
		return false;
	}

	public override int GetProcessInventoryItemCapacity(IInventory inventory, int slotIndex, int value)
	{
		if (value == ItemsBlockManager.GetItemID("FixedBullet") && RifleBlock.GetLoadState(Terrain.ExtractData(inventory.GetSlotValue(slotIndex))) != RifleBlock.LoadState.Bullet2)
		{
			return 1;
		}
		return 0;
	}

	public override void ProcessInventoryItem(IInventory inventory, int slotIndex, int value, int count, int processCount, out int processedValue, out int processedCount)
	{
		processedValue = value;
		processedCount = count;
		if (processCount == 1)
		{
			RifleBlock.LoadState loadState = RifleBlock.GetLoadState(Terrain.ExtractData(inventory.GetSlotValue(slotIndex)));
			switch (loadState)
			{
			case RifleBlock.LoadState.Empty:
				loadState = RifleBlock.LoadState.Bullet;
				break;
			case RifleBlock.LoadState.Bullet:
				loadState = RifleBlock.LoadState.Bullet2;
				break;
			}
			processedValue = 0;
			processedCount = 0;
			inventory.RemoveSlotItems(slotIndex, 1);
			inventory.AddSlotItems(slotIndex, Terrain.MakeBlockValue(RifleBlock.Index, 0, RifleBlock.SetLoadState(Terrain.ExtractData(inventory.GetSlotValue(slotIndex)), loadState)), 1);
		}
	}
}
