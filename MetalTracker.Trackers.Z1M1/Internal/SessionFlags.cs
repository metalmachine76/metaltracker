namespace MetalTracker.Trackers.Z1M1.Internal
{
	internal class SessionFlags
	{
		public bool ZeldaQ2 { get; set; }

		public bool OverworldMirrored { get; set; }

		public bool[] DungeonsMirrored { get; set; } = new bool[9];

		public bool ZebesMirrored { get; set; }

		public bool DungeonEntrancesShuffled { get; set; }

		public bool OtherEntrancesShuffled { get; set; }

		public int DungeonRoomShuffleMode { get; set; }

		public int ZebesRoomShuffleMode { get; set; }
	}
}
