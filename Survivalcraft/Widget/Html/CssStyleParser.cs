using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Engine;

namespace Game {
    // style 属性内联 CSS 子集解析：HtmlAgilityPack 只给原始属性串（无任何 CSS 能力，Fizzler 是
    // 选择器不是声明），这里手写声明切分 + 盒简写 TRBL 展开 + px 长度。定位：只认引擎消费的
    // 属性族（本波 padding/margin，border-* 波续用同一入口）；em/%/rem/pt 等单位不支持、含无效值
    // 的声明整条丢弃；缺属性=引擎默认（不造浏览器 UA 默认值表——computed-style 口径留给将来
    // 基类 Padding 版本，本类只报"作者写了什么"）
    internal static class CssStyleParser {
        // 解析 style 串为声明序列（保序，级联=后声明覆盖前声明）：单遍 O(n) 扫描，';' 切声明、
        // 首个 ':' 切属性值，属性名小写、两侧 trim；扫描时同步剥离 /*…*/ 注释（注释可出现在
        // 任意位置、可含分号，未闭合注释按吃到串尾处理——CSS 规范同款）。空段/无冒号/空名空值
        // 跳过。值内分号（url data-uri 等）不支持——本子集消费的盒属性值不出现分号，留档
        public static List<KeyValuePair<string, string>> ParseDeclarations(string style) {
            List<KeyValuePair<string, string>> declarations = [];
            if (string.IsNullOrEmpty(style)) {
                return declarations;
            }
            StringBuilder declaration = new();
            for (int i = 0; i < style.Length; i++) {
                char c = style[i];
                if (c == '/' && i + 1 < style.Length && style[i + 1] == '*') {
                    int end = style.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    if (end < 0) {
                        break;
                    }
                    i = end + 1;
                    continue;
                }
                if (c == ';') {
                    EmitDeclaration(declaration, declarations);
                    continue;
                }
                declaration.Append(c);
            }
            // 尾段无 ';' 收尾；未闭合注释 break 后同样落到这里（注释前已累积的内容仍生效）
            EmitDeclaration(declaration, declarations);
            return declarations;
        }

        private static void EmitDeclaration(StringBuilder text, List<KeyValuePair<string, string>> declarations) {
            string part = text.ToString();
            text.Clear();
            int colon = part.IndexOf(':');
            if (colon <= 0) {
                return;
            }
            string name = part[..colon].Trim().ToLowerInvariant();
            string value = part[(colon + 1)..].Trim();
            if (name.Length == 0 || value.Length == 0) {
                return;
            }
            declarations.Add(new KeyValuePair<string, string>(name, value));
        }

        // 盒属性查询：prefix="padding"/"margin"，按级联序合并 `prefix` 1~4 值简写与
        // `prefix-left/top/right/bottom` 单边声明（后声明覆盖先声明，CSS 级联口径），展开为四边。
        // 无任何有效相关声明返回 false。简写任一值非法 → 整条简写丢弃（CSS 简写原子性）；
        // 单边非法仅丢该条。输出 Vector4 轴序 = Widget.Margin4（X=左 Y=上 Z=右 W=下）
        public static bool TryGetBox(string style, string prefix, out Vector4 box) {
            float? left = null;
            float? top = null;
            float? right = null;
            float? bottom = null;
            foreach (KeyValuePair<string, string> declaration in ParseDeclarations(style)) {
                if (declaration.Key == prefix) {
                    string[] values = declaration.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    // TRBL 展开：1=四边；2=上下 左右；3=上 左右 下；4=上右下左。
                    // 简写原子性：先全部解析进临时量，全成功才写入四边（中途失败整条丢弃，
                    // 无部分生效——审查 P0：先写后析会让非法简写的前缀值泄漏）
                    if (values.Length is < 1 or > 4
                        || !TryParseLength(values[0], out float first)) {
                        continue;
                    }
                    float newTop = first;
                    float newRight = first;
                    float newBottom = first;
                    float newLeft = first;
                    if (values.Length >= 2) {
                        if (!TryParseLength(values[1], out float second)) {
                            continue;
                        }
                        // 两值形：上下=v0、左右=v1（bottom 归 v0——此前误写 v1，探针 padLast MB=32 实证）
                        newBottom = first;
                        newRight = second;
                        newLeft = second;
                    }
                    if (values.Length >= 3) {
                        if (!TryParseLength(values[2], out float third)) {
                            continue;
                        }
                        newBottom = third;
                    }
                    if (values.Length >= 4) {
                        if (!TryParseLength(values[3], out float fourth)) {
                            continue;
                        }
                        newLeft = fourth;
                    }
                    top = newTop;
                    right = newRight;
                    bottom = newBottom;
                    left = newLeft;
                    continue;
                }
                // 单边声明：无效值仅丢该条（不影响已展开的简写值）
                if (declaration.Key == prefix + "-left"
                    && TryParseLength(declaration.Value, out float l)) {
                    left = l;
                }
                else if (declaration.Key == prefix + "-top"
                    && TryParseLength(declaration.Value, out float t)) {
                    top = t;
                }
                else if (declaration.Key == prefix + "-right"
                    && TryParseLength(declaration.Value, out float r)) {
                    right = r;
                }
                else if (declaration.Key == prefix + "-bottom"
                    && TryParseLength(declaration.Value, out float b)) {
                    bottom = b;
                }
            }
            box = new Vector4(left ?? 0f, top ?? 0f, right ?? 0f, bottom ?? 0f);
            return left.HasValue || top.HasValue || right.HasValue || bottom.HasValue;
        }

        // 长度：px（大小写不敏感）/无单位（=px）；em/%/rem/pt 等不支持返回 false。负值接受——
        // 引擎布局是浮点加减，负 margin=间距缩小（CSS 同义）
        public static bool TryParseLength(string value, out float px) {
            string v = value.Trim();
            if (v.EndsWith("px", StringComparison.OrdinalIgnoreCase)) {
                v = v[..^2].Trim();
            }
            return float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out px);
        }
    }
}
