using MetalTracker.Common.Types;

namespace MetalTracker.Games.Zelda.Internal.Types
{
	internal class DungeonRoomState
	{
		// shared state

		public GameExit ExitNorth { get; set; }
		public GameExit ExitSouth { get; set; }
		public GameExit ExitWest { get; set; }
		public GameExit ExitEast { get; set; }
		public DungeonWall WallNorth { get; set; }
		public DungeonWall WallSouth { get; set; }
		public DungeonWall WallWest { get; set; }
		public DungeonWall WallEast { get; set; }
		public GameItem Item1 { get; set; }
		public GameItem Item2 { get; set; }
		public string Transport { get; set; }

		// local state

		/// <summary>
		/// 0 = none
		/// 1 = explored
		/// 2 = ignored
		/// </summary>
		public int Status { get; set; }

		public DungeonRoomState Clone()
		{
			var clone = new DungeonRoomState();

			clone.ExitNorth = this.ExitNorth;
			clone.ExitSouth = this.ExitSouth;
			clone.ExitWest = this.ExitWest;
			clone.ExitEast = this.ExitEast;
			clone.WallNorth = this.WallNorth;
			clone.WallSouth = this.WallSouth;
			clone.WallWest = this.WallWest;
			clone.WallEast = this.WallEast;
			clone.Item1 = this.Item1;
			clone.Item2 = this.Item2;
			clone.Transport = this.Transport;

			return clone;
		}

		public void Mirror()
		{
			var de = this.ExitEast;
			var dw = this.ExitWest;
			var we = this.WallEast;
			var ww = this.WallWest;

			this.ExitEast = dw;
			this.ExitWest = de;
			this.WallEast = ww;
			this.WallWest = we;
		}
	}
}
