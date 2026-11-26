using System;

namespace SCIENEW;

public readonly struct CreditsEntry(string name, string role, string link = "")
{
	public string Name { get; } = name;

	public string Role { get; } = role;

	public string Link { get; } = link ?? string.Empty;

	public bool IsDeveloper => Role.Equals("developer", StringComparison.OrdinalIgnoreCase);

	public bool IsSupporter => Role.Equals("supporter", StringComparison.OrdinalIgnoreCase);
}
