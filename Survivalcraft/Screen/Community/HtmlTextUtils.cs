using HtmlAgilityPack;

namespace Game {
    // 帖子详情 HTML 相关纯函数：可见文本探测、纯文本规整、列表标记计算。
    // 与 widget 构造解耦，便于单测。
    static class HtmlTextUtils {
        // 翻译前置检查：是否存在可翻译的可见文本（script/style 与纯图片不计）。
        public static bool HasVisibleText(string html) {
            if (string.IsNullOrWhiteSpace(html)) {
                return false;
            }
            HtmlDocument document = new();
            document.LoadHtml(html);
            return HasVisibleText(document.DocumentNode);
        }

        static bool HasVisibleText(HtmlNode node) {
            foreach (HtmlNode child in node.ChildNodes) {
                switch (child.NodeType) {
                    case HtmlNodeType.Text:
                        if (!string.IsNullOrWhiteSpace(HtmlEntity.DeEntitize(((HtmlTextNode)child).Text))) {
                            return true;
                        }
                        break;
                    case HtmlNodeType.Comment: break;
                    case HtmlNodeType.Element:
                        if (child.Name?.ToLowerInvariant() is not ("script" or "style")
                            && HasVisibleText(child)) {
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        // 纯文本（Gitee release notes 等）逐行解析："- "/"* " 开头识别为列表项，空行丢弃。
        // 渲染（widget 产出）由 CommunityPostDetailsDialog.AppendPlainText 按此结果执行。
        public static List<(bool IsListItem, string Text)> SplitPlainTextLines(string text) {
            List<(bool, string)> lines = [];
            if (string.IsNullOrWhiteSpace(text)) {
                return lines;
            }
            foreach (string rawLine in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')) {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line)) {
                    continue;
                }
                if (line.StartsWith("- ", StringComparison.Ordinal)
                    || line.StartsWith("* ", StringComparison.Ordinal)) {
                    lines.Add((true, line[2..].Trim()));
                }
                else {
                    lines.Add((false, line));
                }
            }
            return lines;
        }

        // 无序列表标记按层级：• / ◦ / 第三级及更深 ▪；有序列表 "1." "2." …（number 由调用方按列表维护计数）。
        public static string GetListMarker(bool ordered, int number, int depth) {
            if (ordered) {
                return $"{number}.";
            }
            return depth switch {
                <= 1 => "•",
                2 => "◦",
                _ => "▪"
            };
        }
    }
}