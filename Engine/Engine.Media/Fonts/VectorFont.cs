using System;
using System.Collections.Generic;
using FontStashSharp;
using FontStashSharp.Interfaces;

namespace Engine.Media {
    /// <summary>
    /// 链条目：一个字体文件（ttc/可变字体）+ 其各样式面规格（FontManager 构建链用）
    /// </summary>
    public class ChainEntry {
        public byte[] Data;

        public string Origin; // 描述用（"Pericles"/"system:msyh"/"user:xxx.ttf"）

        public IReadOnlyDictionary<VectorFontStyle, FontSourceSpec> StyleFaces;
    }

    /// <summary>
    /// 矢量字体链：每样式一个 FontSystem（内部按条目序 fallback）。
    /// 样式缺失→IsXxxAvailable=false，渲染走降级路径（Task 5）。
    /// </summary>
    public class VectorFont : IDisposable {
        public const float DefaultSizeValue = 21f;   // ≈CJK 视觉 19px（rem 锚）
        public const float LineHeightValue = 26f;    // 现位图行距等效

        readonly FontSystem[] _systems = new FontSystem[4];
        readonly EngineSharpAstroLoader[] _loaders = new EngineSharpAstroLoader[4];
        readonly List<IFontSource> _regularSources = new List<IFontSource>();
        BitmapFont _fallbackBitmapFont;

        public bool IsBoldAvailable => _systems[(int)VectorFontStyle.Bold] != null;
        public bool IsItalicAvailable => _systems[(int)VectorFontStyle.Italic] != null;
        public bool IsBoldItalicAvailable => _systems[(int)VectorFontStyle.BoldItalic] != null;
        public float DefaultSize => DefaultSizeValue;
        public float LineHeight => LineHeightValue;
        public BitmapFont FallbackBitmapFont { get => _fallbackBitmapFont; set => _fallbackBitmapFont = value; }
        internal IReadOnlyList<IFontSource> RegularSources => _regularSources;

        public VectorFont(IReadOnlyList<ChainEntry> entries, BitmapFont fallbackBitmapFont) {
            ArgumentNullException.ThrowIfNull(entries);
            _fallbackBitmapFont = fallbackBitmapFont;
            // 每样式：收集有该样式面的条目 → 一个 FontSystem，AddFont 顺序=链序
            for (int i = 0; i < 4; i++) {
                var style = (VectorFontStyle)i;
                var specs = new List<FontSourceSpec>();
                var datas = new List<byte[]>();
                foreach (var entry in entries) {
                    if (entry.StyleFaces.TryGetValue(style, out var spec)) {
                        specs.Add(spec);
                        datas.Add(entry.Data);
                    }
                }
                if (specs.Count == 0) {
                    continue; // 该样式整链缺失
                }
                // 每样式一个 loader 实例（spec 队列=链序，AddFont→Load 串行一一对应），
                // Regular 的 CreatedSources 供 HasGlyph 查 glyph 覆盖
                var loader = new EngineSharpAstroLoader(specs);
                var settings = new FontSystemSettings {
                    TextureWidth = 1024,
                    TextureHeight = 1024,
                    GlyphRenderResult = GlyphRenderResult.NonPremultiplied,
                    FontLoader = loader
                };
                var system = new FontSystem(settings);
                foreach (var data in datas) {
                    system.AddFont(data);
                }
                _systems[i] = system;
                _loaders[i] = loader;
                if (style == VectorFontStyle.Regular) {
                    // 复制 source 引用供 HasGlyph（loader.Load 顺序=AddFont 顺序=链序）
                    foreach (var source in loader.CreatedSources) {
                        _regularSources.Add(source);
                    }
                }
            }
        }

        /// <summary>
        /// Regular 链是否覆盖该码点（run 切分用，按链序任一源命中即可）
        /// </summary>
        public bool HasGlyph(int codepoint) {
            foreach (var source in _regularSources) {
                if (source.GetGlyphId(codepoint) != null) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 度量：与 DrawText 同一降级解析（缺失样式→Regular），保证度量的就是将绘制的矢量链；
        /// 仅 Regular 链整链缺失时返回 Zero，调用方退位图度量
        /// </summary>
        public Vector2 MeasureString(string text, float size, bool bold = false, bool italic = false) {
            var font = GetStyleFont(ResolveStyle(bold, italic, degrade: true), size);
            if (font == null) {
                return Vector2.Zero; // 整链缺失，调用方退位图度量
            }
            var size2 = font.MeasureString(text);
            return new Vector2(size2.X, size2.Y);
        }

        internal SpriteFontBase GetStyleFont(VectorFontStyle style, float size)
            => _systems[(int)style]?.GetFont(size);

        /// <summary>
        /// 样式解析：degrade=false 取精确样式；degrade=true 按可用性沿链回退
        /// （boldItalic→BoldItalic→Bold→Regular；bold→Bold→Regular；italic→Italic→Regular），
        /// 返回值对应实际存在的 FontSystem（Regular 链缺失时返回的 Regular 解析为 null，由调用方退位图）
        /// </summary>
        internal VectorFontStyle ResolveStyle(bool bold, bool italic, bool degrade) {
            if (bold && italic) {
                if (!degrade || IsBoldItalicAvailable) {
                    return VectorFontStyle.BoldItalic;
                }
                if (IsBoldAvailable) {
                    return VectorFontStyle.Bold;
                }
                return VectorFontStyle.Regular;
            }
            if (bold) {
                if (!degrade || IsBoldAvailable) {
                    return VectorFontStyle.Bold;
                }
                return VectorFontStyle.Regular;
            }
            if (italic) {
                if (!degrade || IsItalicAvailable) {
                    return VectorFontStyle.Italic;
                }
                return VectorFontStyle.Regular;
            }
            return VectorFontStyle.Regular;
        }

        public void Dispose() {
            foreach (var system in _systems) {
                system?.Dispose();
            }
        }
    }
}
