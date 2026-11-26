using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SCIENEW;

public static class IEVersionComparer
{
	public enum ChannelRank
	{
		Dev,
		Preview,
		Alpha,
		Beta,
		Unknown,
		Rc,
		Release
	}

	public readonly struct SuffixInfo(ChannelRank rank, int revision, string normalizedBase)
	{
		public ChannelRank Rank { get; } = rank;

		public int Revision { get; } = revision;

		public string NormalizedBase { get; } = normalizedBase;
	}

	public readonly struct ParsedVersion
	{
		public static ParsedVersion DefaultRelease { get; } = new ParsedVersion(new int[4], new SuffixInfo(ChannelRank.Release, 0, "release"));

		public int[] Segments { get; }

		public ChannelRank ChannelRank { get; }

		public int ChannelRevision { get; }

		public string NormalizedSuffix { get; }

		public ParsedVersion(int[] segments, SuffixInfo suffix)
		{
			Segments = segments;
			ChannelRank = suffix.Rank;
			ChannelRevision = suffix.Revision;
			NormalizedSuffix = suffix.NormalizedBase;
		}
	}

	private static readonly Regex s_versionPattern = new Regex("^v?(?<nums>\\d+(?:\\.\\d+)*)(?:[-_\\s]+(?<suffix>.+))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

	public static int Compare(string a, string b)
	{
		ParsedVersion parsedVersion = Parse(a);
		ParsedVersion parsedVersion2 = Parse(b);
		for (int i = 0; i < 4; i++)
		{
			int num = parsedVersion.Segments[i].CompareTo(parsedVersion2.Segments[i]);
			if (num != 0)
			{
				return num;
			}
		}
		int num2 = parsedVersion.ChannelRank.CompareTo(parsedVersion2.ChannelRank);
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = parsedVersion.ChannelRevision.CompareTo(parsedVersion2.ChannelRevision);
		if (num3 != 0)
		{
			return num3;
		}
		return string.CompareOrdinal(parsedVersion.NormalizedSuffix, parsedVersion2.NormalizedSuffix);
	}

	public static ParsedVersion Parse(string version)
	{
		if (string.IsNullOrWhiteSpace(version))
		{
			return ParsedVersion.DefaultRelease;
		}
		string text = version.Trim();
		int num = text.IndexOf('+');
		if (num >= 0)
		{
			text = text.Substring(0, num);
		}
		Match match = s_versionPattern.Match(text);
		if (!match.Success)
		{
			return new ParsedVersion(new int[4], ResolveSuffix(text));
		}
		int[] segments = PadSegments(match.Groups["nums"].Value);
		string raw = (match.Groups["suffix"].Success ? match.Groups["suffix"].Value : string.Empty);
		return new ParsedVersion(segments, ResolveSuffix(raw));
	}

	private static int[] PadSegments(string nums)
	{
		string[] array = nums.Split('.', StringSplitOptions.RemoveEmptyEntries);
		int[] array2 = new int[4];
		int num = Math.Min(4, array.Length);
		for (int i = 0; i < num; i++)
		{
			if (!int.TryParse(array[i], out var result) || result < 0)
			{
				result = 0;
			}
			array2[i] = result;
		}
		return array2;
	}

	private static SuffixInfo ResolveSuffix(string raw)
	{
		string text = NormalizeSuffix(raw);
		bool flag = string.IsNullOrEmpty(text);
		if (!flag)
		{
			bool flag2;
			switch (text)
			{
			case "release":
			case "stable":
			case "final":
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		if (flag)
		{
			return new SuffixInfo(ChannelRank.Release, 0, "release");
		}
		SplitTrailingDigits(text, out string basePart, out int revision);
		if (string.IsNullOrEmpty(basePart))
		{
			return new SuffixInfo(ChannelRank.Unknown, 0, text);
		}
		ChannelRank rank = MapChannel(basePart);
		return new SuffixInfo(normalizedBase: CanonicalSuffix(rank, basePart), rank: rank, revision: revision);
	}

	private static string CanonicalSuffix(ChannelRank rank, string basePart)
	{
		return rank switch
		{
			ChannelRank.Dev => "dev",
			ChannelRank.Preview => "preview",
			ChannelRank.Alpha => "alpha",
			ChannelRank.Beta => "beta",
			ChannelRank.Rc => "rc",
			ChannelRank.Release => "release",
			_ => basePart,
		};
	}

	private static void SplitTrailingDigits(string normalized, out string basePart, out int revision)
	{
		int num = normalized.Length;
		while (num > 0 && char.IsDigit(normalized[num - 1]))
		{
			num--;
		}
		basePart = normalized.Substring(0, num);
		if (num < normalized.Length)
		{
			int num2 = num;
			if (int.TryParse(normalized.Substring(num2, normalized.Length - num2), out var result))
			{
				revision = result;
				return;
			}
		}
		revision = 0;
	}

	private static ChannelRank MapChannel(string basePart)
	{
		switch (basePart)
		{
		case "dev":
		case "snapshot":
		case "nightly":
		case "canary":
			return ChannelRank.Dev;
		case "pre":
		case "preview":
			return ChannelRank.Preview;
		case "alpha":
		case "a":
			return ChannelRank.Alpha;
		case "openbeta":
		case "b":
		case "beta":
			return ChannelRank.Beta;
		case "rc":
		case "releasecandidate":
			return ChannelRank.Rc;
		case "release":
		case "stable":
		case "final":
			return ChannelRank.Release;
		default:
			return ChannelRank.Unknown;
		}
	}

	public static string NormalizeSuffix(string suffix)
	{
		if (string.IsNullOrEmpty(suffix))
		{
			return string.Empty;
		}
		return new string(suffix.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
	}
}
