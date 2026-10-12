using System.Runtime.CompilerServices;
using Engine.Graphics;

namespace Game {
    /// <summary>
    /// 独立即时批次键：TexturedBatch 的可变参数三元组，用于寻址按参数缓存的共享批次；
    /// 后三项（layer=0、RasterizerState.CullCounterClockwiseScissor、SamplerState.PointClamp）
    /// 在所有独立绘制路径中恒定，由 <see cref="BlocksManager.GetTexturedBatch"/> 统一补齐。
    /// depth/blend 为 null 时构造器归一化为引擎缺省值（深度=Default、混合=AlphaBlend），
    /// 与 PrimitivesRenderer3D.TexturedBatch 进 FindTexturedBatch 前的补缺完全一致——
    /// 键空间与物理批次空间 1:1，解析后相同的键（如 Cube/MeshAlphaTest/Flat）在账本中合并为同一键累加；
    /// state 对象是引擎单例，按引用判等。
    /// 专用 class：键实例全部来自下方静态字段（进程内单例），但判等/哈希按字段值语义（!= 运算符按值），
    /// 防引用身份混入账本键。
    /// </summary>
    public sealed class TexturedBatchKey : IEquatable<TexturedBatchKey> {
        /// <summary>六面体：alphaTest，默认深度+默认混合（与 MeshAlphaTest、Flat 解析为同一物理批次）。</summary>
        public static readonly TexturedBatchKey Cube = new(true, null, null);

        /// <summary>网格不透明/镂空子集：alphaTest，默认深度+默认混合（与 Cube 同一物理批次）。</summary>
        public static readonly TexturedBatchKey MeshAlphaTest = new(true, DepthStencilState.Default, null);

        /// <summary>网格透明子集：无 alphaTest，DepthRead+默认混合。</summary>
        public static readonly TexturedBatchKey MeshTransparent = new(false, DepthStencilState.DepthRead, null);

        /// <summary>交叉面片：alphaTest，默认深度+默认混合（与 Cube 同一物理批次）。</summary>
        public static readonly TexturedBatchKey Flat = new(true, null, BlendState.AlphaBlend);

        public readonly bool UseAlphaTest;
        public readonly DepthStencilState DepthStencilState;
        public readonly BlendState BlendState;

        public TexturedBatchKey(bool useAlphaTest, DepthStencilState depthStencilState, BlendState blendState) {
            UseAlphaTest = useAlphaTest;
            // 归一化到物理批次的实际解析参数：null → PrimitivesRenderer3D.TexturedBatch 的补缺值。
            // 键不相归一会把同批容量按逻辑键拆开，预扩容取 max 漏掉 sum 的部分。
            DepthStencilState = depthStencilState ?? DepthStencilState.Default;
            BlendState = blendState ?? BlendState.AlphaBlend;
        }

        public bool Equals(TexturedBatchKey other) =>
            !ReferenceEquals(other, null)
            && UseAlphaTest == other.UseAlphaTest
            && ReferenceEquals(DepthStencilState, other.DepthStencilState)
            && ReferenceEquals(BlendState, other.BlendState);

        public override bool Equals(object obj) => obj is TexturedBatchKey other && Equals(other);

        public static bool operator ==(TexturedBatchKey left, TexturedBatchKey right) =>
            ReferenceEquals(left, right) || (!ReferenceEquals(left, null) && left.Equals(right));

        public static bool operator !=(TexturedBatchKey left, TexturedBatchKey right) => !(left == right);

        /// <summary>
        /// 31 进制逐字段累加；state 对象按引用身份取哈希（引擎单例，进程内稳定）。
        /// 归一化后两 state 字段恒非 null，null 分支仅作防御保留；
        /// 与 Equals 的 ReferenceEquals 判等口径一致：相等键必然持有相同 state 引用，哈希必相等。
        /// </summary>
        public override int GetHashCode() {
            unchecked {
                int hash = UseAlphaTest ? unchecked((int)0x9E3779B9) : unchecked((int)0x7F4A7C15);
                hash = hash * 31 + (DepthStencilState != null ? RuntimeHelpers.GetHashCode(DepthStencilState) : 0);
                hash = hash * 31 + (BlendState != null ? RuntimeHelpers.GetHashCode(BlendState) : 0);
                return hash;
            }
        }
    }

    /// <summary>
    /// 一个方块值在独立绘制场景的自报容量需求（上界）：一组「批次键 → 顶点/索引数」条目。
    /// 返回 null 表示不参与预扩容记账（走原生逐块扩容行为）。
    /// </summary>
    public sealed class BlockDrawDemand {
        public struct Entry {
            public TexturedBatchKey BatchKey;
            public int VertexCount;
            public int IndexCount;
        }

        readonly Entry[] m_entries;

        /// <summary>普通六面体：6 面 = 24 顶点/36 索引。</summary>
        public static readonly BlockDrawDemand Cube = new((TexturedBatchKey.Cube, 24, 36));

        /// <summary>交叉面片：billboard 单 quad，上界 2 quad（无 BillboardDirection 时画两面）。</summary>
        public static readonly BlockDrawDemand Flat = new((TexturedBatchKey.Flat, 8, 12));

        public BlockDrawDemand(params (TexturedBatchKey BatchKey, int VertexCount, int IndexCount)[] entries) {
            m_entries = new Entry[entries.Length];
            for (int i = 0; i < entries.Length; i++) {
                m_entries[i] = new Entry { BatchKey = entries[i].BatchKey, VertexCount = entries[i].VertexCount, IndexCount = entries[i].IndexCount };
            }
        }

        public IReadOnlyList<Entry> Entries => m_entries;

        public static BlockDrawDemand FromBlockMesh(TexturedBatchKey batchKey, BlockMesh blockMesh) =>
            blockMesh == null ? null : new BlockDrawDemand((batchKey, blockMesh.Vertices.Count, blockMesh.Indices.Count));

        /// <summary>按键合并多份需求（同键顶点/索引累加），供家具等多子集几何求总。</summary>
        public static BlockDrawDemand Sum(params BlockDrawDemand[] parts) {
            Dictionary<TexturedBatchKey, (int Verts, int Idxs)> totals = [];
            foreach (BlockDrawDemand part in parts) {
                if (part == null) {
                    continue;
                }
                foreach (Entry entry in part.Entries) {
                    (int verts, int idxs) = totals.TryGetValue(entry.BatchKey, out (int, int) value) ? value : (0, 0);
                    totals[entry.BatchKey] = (verts + entry.VertexCount, idxs + entry.IndexCount);
                }
            }
            if (totals.Count == 0) {
                return null;
            }
            (TexturedBatchKey, int, int)[] tuples = new (TexturedBatchKey, int, int)[totals.Count];
            int index = 0;
            foreach (KeyValuePair<TexturedBatchKey, (int Verts, int Idxs)> pair in totals) {
                tuples[index++] = (pair.Key, pair.Value.Verts, pair.Value.Idxs);
            }
            return new BlockDrawDemand(tuples);
        }
    }

    /// <summary>
    /// 方块绘制容量需求账本：随实体（掉落物/抛射物）增删按值累加/扣减，
    /// 绘制前按键一次性 <see cref="Engine.Graphics.BaseTexturedBatch.EnsureCapacity"/>。
    /// 与粒子系统的 m_quadDemand 账本同构。
    /// </summary>
    public class BlockDrawDemandLedger {
        readonly Dictionary<TexturedBatchKey, (int Verts, int Idxs)> m_demand = [];

        public IReadOnlyDictionary<TexturedBatchKey, (int Verts, int Idxs)> Demand => m_demand;

        public void Book(BlockDrawDemand demand, int sign) {
            if (demand == null) {
                return;
            }
            foreach (BlockDrawDemand.Entry entry in demand.Entries) {
                (int verts, int idxs) = m_demand.TryGetValue(entry.BatchKey, out (int, int) value) ? value : (0, 0);
                verts += sign * entry.VertexCount;
                idxs += sign * entry.IndexCount;
                if (verts <= 0
                    && idxs <= 0) {
                    m_demand.Remove(entry.BatchKey);
                }
                else {
                    m_demand[entry.BatchKey] = (verts, idxs);
                }
            }
        }
    }
}
