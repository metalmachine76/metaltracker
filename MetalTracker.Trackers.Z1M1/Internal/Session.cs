using System.Collections.Generic;
using MetalTracker.Games.Metroid.Types;
using MetalTracker.Games.Zelda.Types;

namespace MetalTracker.Trackers.Z1M1.Internal
{
	internal class Session
	{
		public SessionFlags Flags { get; set; }
		public OverworldMapState Overworld { get; set; }
		public DungeonMapState Level1 { get; set; }
		public DungeonMapState Level2 { get; set; }
		public DungeonMapState Level3 { get; set; }
		public DungeonMapState Level4 { get; set; }
		public DungeonMapState Level5 { get; set; }
		public DungeonMapState Level6 { get; set; }
		public DungeonMapState Level7 { get; set; }
		public DungeonMapState Level8 { get; set; }
		public DungeonMapState Level9 { get; set; }
		public ZebesMapState Zebes { get; set; }
		public List<InventoryEntry> Inventory { get; set; }
	}
}
