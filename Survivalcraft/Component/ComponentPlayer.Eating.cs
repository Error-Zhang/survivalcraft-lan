using Engine;
using Engine.Audio;

namespace Game {
    public partial class ComponentPlayer {
        private const float EatDuration = 1.35f;
        private const float BiteInterval = 0.29f;
        private const string ChewingSound = "Audio/Creatures/HumanEat/HumanEat1";
        private int m_foodSlot = -1;
        private int m_heldFood;
        private float m_eatingTime;
        private float m_nextBiteTime;
        private double m_nextNetworkEatTime;
        private Sound m_eatingSound;

        private bool UpdateHeldFood(PlayerInput input, float dt, bool interacted) {
            IInventory inventory = ComponentMiner.Inventory;
            Ray3? useRay = input.Aim.HasValue && ComponentInput.IsControlledByTouch && input.Dig.HasValue ? input.Dig : input.Aim;
            if (!useRay.HasValue || interacted || input.Hit.HasValue || input.Drop || inventory == null
                || ComponentHealth.Health <= 0f || ComponentSleep.SleepFactor > 0f
                || ComponentGui.ModalPanelWidget != null || DialogsManager.HasDialogs(GuiWidget)) {
                ResetEating();
                return false;
            }

            int slot = inventory.ActiveSlotIndex;
            int value = ComponentMiner.ActiveBlockValue;
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
            if (slot < 0 || !CanEatHeldFood(block, value)
                || ComponentMiner.Raycast(useRay.Value, RaycastMode.Interaction) is not Ray3) {
                ResetEating();
                return false;
            }

            if (slot != m_foodSlot || value != m_heldFood) {
                ResetEating();
                m_foodSlot = slot;
                m_heldFood = value;
                m_nextBiteTime = 0.25f;
            }

            float previousTime = m_eatingTime;
            m_eatingTime += dt;
            if (previousTime < 0.18f && m_eatingTime >= 0.18f && SettingsManager.SoundsVolume > 0f) {
                m_eatingSound ??= new Sound(ContentManager.Get<SoundBuffer>(ChewingSound));
                m_eatingSound.Volume = 0.75f * SettingsManager.SoundsVolume;
                m_eatingSound.Pitch = 1.85f;
                m_eatingSound.Play();
            }
            float lift = MathUtils.Saturate(m_eatingTime / 0.2f);
            float bob = MathF.Sin(m_eatingTime * MathF.Tau / BiteInterval) * 0.025f;
            if (ComponentInput.IsControlledByVr && Entity.FindComponent<ComponentVrHandsModel>() is { } vrHands) {
                vrHands.ItemOffsetOrder = new Vector3(-0.04f, 0.10f + bob, 0.03f) * lift;
                vrHands.ItemRotationOrder = new Vector3(-0.25f, 0f, 0f) * lift;
            }
            else if (Entity.FindComponent<ComponentFirstPersonModel>() is { } firstPerson) {
                firstPerson.ItemOffsetOrder = new Vector3(-0.30f, 0.34f + bob, 0.10f) * lift;
                firstPerson.ItemRotationOrder = new Vector3(-0.28f, 0.10f, -0.30f) * lift;
            }
            ComponentCreatureModel.AimHandAngleOrder = (1.45f + 3f * bob) * lift;
            ComponentCreatureModel.InHandItemRotationOrder = new Vector3(-0.45f, 0f, 0.15f) * lift;

            if (m_eatingTime >= m_nextBiteTime && m_eatingTime < EatDuration) {
                m_nextBiteTime += BiteInterval;
                ShowFoodCrumbs(value);
            }

            if (m_eatingTime >= EatDuration) {
                bool isClient = NetworkManager.IsClientRunning;
                bool eatenOrRequested = isClient || TryEatHeldFood(slot, value);
                if (eatenOrRequested && (isClient || NetworkManager.IsServerRunning)) {
                    Vector3 direction = useRay.Value.Direction;
                    ModsManager.HookAction("OnNetworkPlayerEat", loader => loader.OnNetworkPlayerEat(this, slot, value, direction));
                }
                ResetEating();
            }
            return true;
        }

        private bool CanEatHeldFood(Block block, int value) =>
            value != 0
            && !block.IsAimable_(value)
            && block.GetNutritionalValue(value) > 0f
            && ComponentVitalStats.Food < 0.98f
            && (!ComponentSickness.IsSick || block.GetSicknessProbability(value) <= 0f);

        private void ResetEating() {
            if (m_foodSlot >= 0) {
                m_eatingSound?.Stop();
            }
            m_foodSlot = -1;
            m_heldFood = 0;
            m_eatingTime = 0f;
        }

        public bool TryEatNetworkFood(int slot, int value, Vector3 direction) {
            float directionLength = direction.LengthSquared();
            if (!PlayerData.IsReadyForPlaying || ComponentHealth.Health <= 0f || ComponentSleep.SleepFactor > 0f
                || m_subsystemTime.GameTime < m_nextNetworkEatTime || !float.IsFinite(directionLength) || directionLength < 1E-06f
                || ComponentMiner.Raycast(new Ray3(ComponentCreatureModel.EyePosition, direction), RaycastMode.Interaction) is not Ray3) {
                return false;
            }
            if (!TryEatHeldFood(slot, value)) {
                return false;
            }
            m_nextNetworkEatTime = m_subsystemTime.GameTime + EatDuration - 0.2f;
            return true;
        }

        private bool TryEatHeldFood(int slot, int value) {
            IInventory inventory = ComponentMiner.Inventory;
            if (inventory == null || slot != inventory.ActiveSlotIndex || slot < 0 || slot >= inventory.SlotsCount
                || inventory.GetSlotValue(slot) != value || inventory.GetSlotCount(slot) < 1) {
                return false;
            }
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
            if (!CanEatHeldFood(block, value) || block.Eat(ComponentVitalStats, value)) {
                return false;
            }
            bool eaten = PlayerData.IsMainPlayer
                ? ComponentVitalStats.EatWithoutSound(value)
                : ComponentVitalStats.Eat(value);
            if (!eaten) {
                return false;
            }
            if (inventory is not ComponentCreativeInventory && inventory.GetSlotValue(slot) == value
                && inventory.RemoveSlotItems(slot, 1) == 1 && block is BucketBlock) {
                int emptyBucket = BlocksManager.GetBlockIndex<EmptyBucketBlock>();
                if (inventory.GetSlotCount(slot) == 0 && inventory.GetSlotCapacity(slot, emptyBucket) > 0) {
                    inventory.AddSlotItems(slot, emptyBucket, 1);
                }
                else if (ComponentInventoryBase.AcquireItems(inventory, emptyBucket, 1) > 0) {
                    m_subsystemPickables.AddPickable(emptyBucket, 1, ComponentBody.Position + Vector3.UnitY, null, null, Entity);
                }
            }
            return true;
        }

        public void ShowNetworkEating(int value) {
            m_subsystemAudio.PlaySound(ChewingSound, 0.7f, 0.89f, ComponentBody.Position, 2f, 0f);
            ShowFoodCrumbs(value);
        }

        private void ShowFoodCrumbs(int value) {
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
            if (block is BucketBlock) {
                return;
            }
            Matrix eye = Matrix.CreateFromQuaternion(ComponentCreatureModel.EyeRotation);
            Vector3 position = ComponentCreatureModel.EyePosition + 0.52f * eye.Forward + 0.13f * eye.Right - 0.18f * eye.Up;
            ParticleSystemBase crumbs = block.CreateDebrisParticleSystem(m_subsystemTerrain, position, value, 0.12f);
            if (crumbs != null) {
                Project.FindSubsystem<SubsystemParticles>(true).AddParticleSystem(crumbs);
            }
        }

        public override void OnEntityRemoved() {
            ResetEating();
            Utilities.Dispose(ref m_eatingSound);
            base.OnEntityRemoved();
        }
    }
}
