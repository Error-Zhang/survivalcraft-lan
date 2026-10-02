using Engine;
using Engine.Media;

namespace Game {
    // 富文本排版：spans 摊平为字符单元 → 贪心断行（拉丁词回扫 + CJK 禁则，复用 FontTextWidget 规则）→ 按 span 组装 run
    // 字符推进量公式必须与 FontBatch2D.QueueLine 一致：(glyph.Width - kerning + spacing) * scale，
    // 否则排版宽度与实际绘制宽度漂移
    public class RichTextLayout {
        public BitmapFont Font { get; set; }

        public Vector2 FontSpacing { get; set; }

        public List<TextSpan> Spans { get; set; } = [];

        public bool WordWrap { get; set; }

        public int MaxLines { get; set; } = int.MaxValue;

        public bool Ellipsis { get; set; }

        public float DefaultFontScale { get; set; } = 1f;

        public List<TextLine> Lines { get; } = [];

        public Vector2 MeasuredSize { get; private set; }

        readonly struct Cell(char c, TextSpan span, int sequence, bool isTabSpace = false) {
            public readonly char C = c;

            public readonly TextSpan Span = span;

            // 同一 span 的连续源字符共享同一 sequence，kerning 只在同 sequence 相邻字符间生效（与 QueueLine 的 run 内 kerning 对齐）
            public readonly int Sequence = sequence;

            // '\t' 展开出的空格：下一行行首去空白时保留，否则折行点上的 tab 缩进丢失
            public readonly bool IsTabSpace = isTabSpace;
        }

        public float CalculateLineHeight(float fontScale) => (Font.GlyphHeight + Font.Spacing.Y + FontSpacing.Y) * fontScale * Font.Scale;

        public void Layout(float availableWidth, float availableHeight) {
            Lines.Clear();
            MeasuredSize = Vector2.Zero;
            if (Font == null
                || Spans == null
                || Spans.Count == 0) {
                return;
            }
            // 按 '\n' 拆段，段内贪心断行；'\t' 按字符列制展开为空格（每 4 列一个 stop）。
            // 列号按段内累计字符数计，word-wrap 折行后与显示行列号不对应——比例字体下像素对齐需等宽字体另配
            const int tabSize = 4;
            List<List<Cell>> paragraphs = [];
            List<Cell> current = [];
            int sequence = 0;
            foreach (TextSpan span in Spans) {
                string text = span.Text ?? string.Empty;
                foreach (char raw in text) {
                    if (raw == '\r'
                        || raw == '\u200b') {
                        continue;
                    }
                    if (raw == '\n') {
                        paragraphs.Add(current);
                        current = [];
                        continue;
                    }
                    if (raw == '\t') {
                        int spaces = tabSize - current.Count % tabSize;
                        for (int t = 0; t < spaces; t++) {
                            current.Add(new Cell(' ', span, sequence, true));
                        }
                        continue;
                    }
                    current.Add(new Cell(raw == '\u00a0' ? ' ' : raw, span, sequence));
                }
                sequence++;
            }
            paragraphs.Add(current);
            int maxLines = MaxLines;
            if (availableHeight < float.PositiveInfinity) {
                maxLines = MathUtils.Min((int)MathF.Floor(availableHeight / CalculateLineHeight(DefaultFontScale)), maxLines);
            }
            foreach (List<Cell> paragraph in paragraphs) {
                if (Lines.Count >= maxLines) {
                    break;
                }
                if (paragraph.Count == 0) {
                    AddLine([], false);
                    continue;
                }
                int start = 0;
                while (start < paragraph.Count) {
                    int end = paragraph.Count;
                    bool lineEllipsized = false;
                    if (WordWrap) {
                        if (Ellipsis
                            && Lines.Count + 1 >= maxLines) {
                            // 末行直接按减省略号宽适配剩余内容，放不下才截断补 "..."（与 FontTextWidget flag 路径一致）：
                            // 若先按全宽断行再收缩，断行点空隙≥省略号宽时会静默丢内容且无省略号
                            int fitCount = CountFittingCells(paragraph, start, end, availableWidth, reserveEllipsis: true);
                            if (fitCount < end - start) {
                                end = start + fitCount;
                                lineEllipsized = true;
                            }
                        }
                        else {
                            int fitCount = CountFittingCells(paragraph, start, end, availableWidth, reserveEllipsis: false);
                            if (fitCount < end - start) {
                                int breakPosition = FontTextWidget.FindLineBreakPosition(BuildString(paragraph, start, end), MathUtils.Max(fitCount, 1));
                                if (breakPosition <= 0) {
                                    breakPosition = 1;
                                }
                                end = start + breakPosition;
                            }
                        }
                    }
                    else if (Ellipsis) {
                        // 非换行：段落超宽即截断补 "..."（与 FontTextWidget 非换行分支一致）
                        int fitCount = CountFittingCells(paragraph, start, end, availableWidth, reserveEllipsis: true);
                        if (fitCount < end - start) {
                            end = start + fitCount;
                            lineEllipsized = true;
                        }
                    }
                    // 行尾去空白、下一行首去空白（与 FontTextWidget.UpdateLines 一致；tab 展开空格行首保留）
                    while (end > start
                        && char.IsWhiteSpace(paragraph[end - 1].C)) {
                        end--;
                    }
                    int nextStart = MathUtils.Min(end == start ? start + 1 : end, paragraph.Count);
                    while (nextStart < paragraph.Count
                        && char.IsWhiteSpace(paragraph[nextStart].C)
                        && !paragraph[nextStart].IsTabSpace) {
                        nextStart++;
                    }
                    AddLine(paragraph.GetRange(start, end - start), lineEllipsized);
                    if (lineEllipsized) {
                        break;
                    }
                    start = nextStart;
                    if (Lines.Count >= maxLines) {
                        break;
                    }
                }
            }
            // 累计行 y 与测量尺寸；测量时末行不含行尾间距（与 FontTextWidget 一致）
            float y = 0f;
            float maxWidth = 0f;
            for (int i = 0; i < Lines.Count; i++) {
                TextLine line = Lines[i];
                line.Y = y;
                y += line.Height;
                maxWidth = MathUtils.Max(maxWidth, line.Width);
            }
            if (Lines.Count > 0) {
                TextLine last = Lines[^1];
                y -= last.Height - Font.GlyphHeight * last.MaxScale * Font.Scale;
            }
            MeasuredSize = new Vector2(maxWidth, y);
        }

        // 组装一行：同 sequence 连续 cell 归并为一个 run，kerning 记在相邻对的前一个字符上（与 QueueLine 相同）
        void AddLine(List<Cell> cells, bool lineEllipsized) {
            TextLine line = new();
            float maxScale = DefaultFontScale;
            float x = 0f;
            int i = 0;
            while (i < cells.Count) {
                TextSpan span = cells[i].Span;
                float scale = SpanScale(span);
                maxScale = MathUtils.Max(maxScale, scale);
                int j = i;
                while (j + 1 < cells.Count
                    && cells[j + 1].Sequence == cells[i].Sequence) {
                    j++;
                }
                char[] text = new char[j - i + 1];
                float width = 0f;
                for (int k = i; k <= j; k++) {
                    text[k - i] = cells[k].C;
                    width += CharacterAdvance(cells[k].C, scale);
                    if (k < j) {
                        width -= Font.GetKerning(cells[k].C, cells[k + 1].C) * scale * Font.Scale;
                    }
                }
                TextRun run = new() {
                    Span = span,
                    Text = new string(text),
                    X = x,
                    Width = width
                };
                line.Runs.Add(run);
                x += width;
                i = j + 1;
            }
            line.Width = x;
            line.MaxScale = maxScale;
            line.Height = CalculateLineHeight(maxScale);
            line.IsEllipsized = lineEllipsized;
            if (lineEllipsized) {
                // 省略号继承截断点处 span 的视觉样式，但不是链接
                TextSpan source = cells.Count > 0 ? cells[^1].Span : null;
                float sourceScale = source != null ? SpanScale(source) : DefaultFontScale;
                TextSpan ellipsis = new("...") {
                    Color = source?.Color,
                    BackgroundColor = source?.BackgroundColor,
                    FontScale = sourceScale,
                    Style = source?.Style ?? RichTextStyle.Normal
                };
                TextRun run = new() {
                    Span = ellipsis,
                    Text = "...",
                    X = x,
                    Width = MeasureEllipsisWidth(sourceScale)
                };
                line.Runs.Add(run);
                line.Width += run.Width;
            }
            Lines.Add(line);
        }

        // 返回 [start, end) 内总宽不超过 availableWidth 的最大字符数；超宽单字符由调用方强制推进。
        // reserveEllipsis：预留省略号宽度（按当前 cell 的 span 字号度量，与截断后实际追加的省略号字号一致）
        int CountFittingCells(List<Cell> cells, int start, int end, float availableWidth, bool reserveEllipsis) {
            float width = 0f;
            for (int i = start; i < end; i++) {
                float scale = SpanScale(cells[i].Span);
                width += CharacterAdvance(cells[i].C, scale);
                if (i + 1 < end
                    && cells[i + 1].Sequence == cells[i].Sequence) {
                    width -= Font.GetKerning(cells[i].C, cells[i + 1].C) * scale * Font.Scale;
                }
                float limit = reserveEllipsis ? availableWidth - MeasureEllipsisWidth(scale) : availableWidth;
                if (width > limit) {
                    return i - start;
                }
            }
            return end - start;
        }

        float SpanScale(TextSpan span) => span.FontScale > 0f ? span.FontScale : DefaultFontScale;

        float CharacterAdvance(char c, float fontScale) => (Font.GetGlyph(c).Width + Font.Spacing.X + FontSpacing.X) * fontScale * Font.Scale;

        float MeasureEllipsisWidth(float fontScale) {
            const string text = "...";
            return Font.MeasureText(text, 0, text.Length, new Vector2(fontScale), FontSpacing).X;
        }

        static string BuildString(List<Cell> cells, int start, int end) {
            char[] chars = new char[end - start];
            for (int i = start; i < end; i++) {
                chars[i - start] = cells[i].C;
            }
            return new string(chars);
        }
    }
}
