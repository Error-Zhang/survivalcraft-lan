using System.Runtime.CompilerServices;
using FontStashSharp.Interfaces;

namespace Engine.Media {
    /// <summary>
    /// FSS 动态图集纹理工厂：图集即引擎 Texture2D（RGBA8888），与引擎纹理体系同源
    /// </summary>
    public class VectorFontTextureManager : ITexture2DManager {
        // 引擎 Texture2D.SetData 仅支持整纹理上传（TexImage2D 全量替换，无子矩形路径），
        // 而 FSS 的 SetTextureData 按 bounds 子块送数据：为每张图集缓存一份全尺寸 RGBA 缓冲，
        // 子块写回缓存后整体上传。用 ConditionalWeakTable 挂在纹理对象上，
        // FSS 1.6.1 的 ITexture2DManager 无 DisposeTexture，纹理被替换后缓存可随纹理一起回收。
        readonly ConditionalWeakTable<Graphics.Texture2D, byte[]> m_fullBuffers = new();

        public object CreateTexture(int width, int height) => new Graphics.Texture2D(width, height, 1, Graphics.ColorFormat.Rgba8888);

        public System.Drawing.Point GetTextureSize(object texture) {
            var texture2D = (Graphics.Texture2D)texture;
            return new System.Drawing.Point(texture2D.Width, texture2D.Height);
        }

        public void SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data) {
            var texture2D = (Graphics.Texture2D)texture;
            ArgumentNullException.ThrowIfNull(data);
            int expectedBytes = bounds.Width * bounds.Height * 4;
            if (data.Length < expectedBytes) {
                throw new ArgumentException($"Data length {data.Length} is insufficient for {bounds.Width}x{bounds.Height} region (expected {expectedBytes}).");
            }
            byte[] fullBuffer = m_fullBuffers.GetValue(texture2D, static key => new byte[key.Width * key.Height * 4]);
            int sourceStride = bounds.Width * 4;
            for (int y = 0; y < bounds.Height; y++) {
                Buffer.BlockCopy(data, y * sourceStride, fullBuffer, ((bounds.Y + y) * texture2D.Width + bounds.X) * 4, sourceStride);
            }
            texture2D.SetData(0, fullBuffer);
        }
    }
}
