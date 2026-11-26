using SCIENEW.Gen;

namespace SCIENEW.AssetProtection;

internal static class AmpkRuntimeConfig
{
	internal const string SeedPart1 = "LINXIUM";

	internal const string SeedPart2 = "S!C@I#E$N%E^W&";

	internal static bool IsEnabled => true;

	internal static bool VerifyPlainHashOnRuntime => false;

	internal static byte[] MasterKey => KeyGenerator.GenerateKey("LINXIUM", "S!C@I#E$N%E^W&");
}
