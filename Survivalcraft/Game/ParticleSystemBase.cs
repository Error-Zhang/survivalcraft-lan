using Engine.Graphics;

namespace Game {
    /// <summary>按混合模式拆分的 quad 容量需求（供共享批次预扩容）。</summary>
    public struct QuadDemandByBlend {
        public int Additive;
        public int Alpha;

        public static QuadDemandByBlend operator +(QuadDemandByBlend left, QuadDemandByBlend right) =>
            new QuadDemandByBlend { Additive = left.Additive + right.Additive, Alpha = left.Alpha + right.Alpha };

        public static QuadDemandByBlend operator -(QuadDemandByBlend left, QuadDemandByBlend right) =>
            new QuadDemandByBlend { Additive = left.Additive - right.Additive, Alpha = left.Alpha - right.Alpha };
    }

    public abstract class ParticleSystemBase {
        public SubsystemParticles SubsystemParticles;

        public abstract void Draw(Camera camera);

        public abstract bool Simulate(float dt);

        public virtual void OnAdded() { }

        public virtual void OnRemoved() { }

        /// <summary>
        /// 粒子系统自报容量需求：本系统在场上时最多会向哪个纹理批次入队多少 quad。
        /// SubsystemParticles 在系统增删时据此记账，绘制前对共享批次一次性精确预扩容
        /// （避免大批量入队在 DynamicArray 精确扩容策略下触发逐 quad O(N²) 整段拷贝）。
        /// 返回 null 纹理表示不参与预扩容（走原生逐 quad 扩容行为）。
        /// 契约：纹理与容量需求在系统生命周期内须恒定（注册后运行中改动会导致账本增删不对称）。
        /// </summary>
        public virtual Texture2D QuadDemandTexture => null;

        /// <summary>与 <see cref="QuadDemandTexture"/> 同生命周期恒定契约；Additive/Alpha 两侧按各自上界独立报告。</summary>
        public virtual QuadDemandByBlend MaxQuadDemand => default;
    }
}