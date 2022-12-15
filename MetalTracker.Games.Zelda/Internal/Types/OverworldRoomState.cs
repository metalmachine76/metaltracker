using MetalTracker.Common.Types;

namespace MetalTracker.Games.Zelda.Internal.Types
{
	internal class OverworldRoomState
	{
		// shared state

		public GameExit Exit { get; set; }
		public OverworldCave Cave { get; set; }
		public GameItem Item1 { get; set; }
		public GameItem Item2 { get; set; }
		public GameItem Item3 { get; set; }

		// local state

		/// <summary>
		/// 0 = none
		/// 1 = explored
		/// 2 = ignored
		/// </summary>
		public int Status { get; set; }

		public OverworldRoomState Clone()
		{
			var clone = new OverworldRoomState();

			clone.Exit = this.Exit;
			clone.Cave = this.Cave;
			clone.Item1 = this.Item1;
			clone.Item2 = this.Item2;
			clone.Item3 = this.Item3;

			return clone;
		}
	}
}
