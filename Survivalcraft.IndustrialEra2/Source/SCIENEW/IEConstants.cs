namespace SCIENEW;

public static class IEConstants
{
	public static class BlockCategory
	{
		public const string Devices = "Devices";

		public const string Items = "Items";

		public const string Construction = "Construction";

		public const string Minerals = "Minerals";

		public const string Tools = "Tools";

		public const string Weapons = "Weapons";

		public const string Painted = "Painted";

		public const string Plants = "Plants";

		public const string Terrain = "Terrain";
	}

	public static class DeviceDisplayOrder
	{
		public const int Creative = -3;

		public const int Logical = -2;

		public const int Mechanical = 0;

		public const int Electrical = 1;

		public const int Nuclear = 2;

		public const int Pipe = 3;

		public const int Wire = 4;

		public const int AprilFools = 10086;
	}

	public static class Recipaedia
	{
		public const string MatchedResultFluidValues = "SCIENEW.MatchedResultFluidValues";
	}

	public static class VoltNet
	{
		public readonly struct WireSpec(float pl, float ul)
		{
			public float PL { get; } = pl;

			public float UL { get; } = ul;
		}

		public static WireSpec CopperWire => new WireSpec(12f, 1f);

		public static WireSpec SilverWire => new WireSpec(24f, 1f);

		public static WireSpec GoldWire => new WireSpec(8f, 2f);

		public static WireSpec AluminumWire => new WireSpec(8f, 4f);

		public static WireSpec SteelWire => new WireSpec(48f, 4f);

		public static WireSpec WireCube => new WireSpec(16f, 1f);

		public static WireSpec GetWireSpec(Materials material)
		{
			return material switch
			{
				Materials.Copper => CopperWire,
				Materials.Silver => SilverWire,
				Materials.Gold => GoldWire,
				Materials.Aluminum => AluminumWire,
				Materials.Steel => SteelWire,
				_ => CopperWire,
			};
		}
	}
}
