using MetalTracker.Common.Types;

namespace MetalTracker.Games.Zelda.Types
{
    public class DungeonRoomState
	{
		public bool Explored { get; set; }
		public GameDest DestNorth { get; set; }
		public GameDest DestSouth { get; set; }
		public GameDest DestWest { get; set; }
		public GameDest DestEast { get; set; }
		public bool WallNorth { get; set; }
		public bool WallSouth { get; set; }
		public bool WallWest { get; set; }
		public bool WallEast { get; set; }
	}
}
