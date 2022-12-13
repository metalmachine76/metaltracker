using MetalTracker.Common.Types;

namespace MetalTracker.Games.Zelda.Internal.Types
{
	internal class OverworldRoomState
	{
		// shared state

		public bool Ignored { get; set; }
		public GameDest Exit { get; set; }
		public OverworldCave Cave { get; set; }
		public GameItem Item1 { get; set; }
		public GameItem Item2 { get; set; }
		public GameItem Item3 { get; set; }

		// player state

		public bool Explored { get; set; }

		public OverworldRoomState Clone()
		{
			var clone = new OverworldRoomState();

			clone.Ignored = this.Ignored;
			clone.Exit = this.Exit;
			clone.Cave = this.Cave;
			clone.Item1 = this.Item1;
			clone.Item2 = this.Item2;
			clone.Item3 = this.Item3;

			return clone;
		}
	}
}
