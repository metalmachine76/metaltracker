using MetalTracker.Common.Types;

namespace MetalTracker.Games.Metroid.Internal.Types
{
	internal class ZebesRoomState
	{
		// shared state

		public GameDest DestUp { get; set; }
		public GameDest DestDown { get; set; }
		public GameDest DestLeft { get; set; }
		public GameDest DestRight { get; set; }
		public GameItem Item { get; set; }

		// local state

		public bool Explored { get; set; }

		public ZebesRoomState Clone()
		{
			var clone = new ZebesRoomState();

			clone.DestUp = this.DestUp;
			clone.DestDown = this.DestDown;
			clone.DestLeft = this.DestLeft;
			clone.DestRight = this.DestRight;
			clone.Item = Item;

			return clone;
		}

		public void Mirror()
		{
			var dl = DestLeft;
			var dr = DestRight;

			DestLeft = dr;
			DestRight = dl;
		}
	}
}
