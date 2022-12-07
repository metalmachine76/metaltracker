using MetalTracker.Common.Types;

namespace MetalTracker.Games.Zelda.Types
{
    public class DungeonRoomState
	{
		// shared state

		public GameDest DestNorth { get; set; }
		public GameDest DestSouth { get; set; }
		public GameDest DestWest { get; set; }
		public GameDest DestEast { get; set; }
		public GameItem Item1 { get; set; }
		public GameItem Item2 { get; set; }
		public string Transport { get; set; }

		// local state

		public bool Explored { get; set; }

		public DungeonRoomState Clone()
		{
			var clone = new DungeonRoomState();

			clone.DestNorth = this.DestNorth;
			clone.DestSouth = this.DestSouth;
			clone.DestWest = this.DestWest;
			clone.DestEast = this.DestEast;
			clone.Item1 = this.Item1;
			clone.Item2 = this.Item2;
			clone.Transport = this.Transport;

			return clone;
		}

		public void Mirror()
		{
			var e = this.DestEast;
			var w = this.DestWest;
			this.DestEast = w;
			this.DestWest = e;
		}
	}
}
