using MetalTracker.Common.Types;

namespace MetalTracker.Games.Metroid.Internal.Types
{
	internal class ZebesRoomState
	{
		// shared state

		public bool Ignored { get; set; }
		public GameDest ExitUp { get; set; }
		public GameDest ExitDown { get; set; }
		public GameDest ExitLeft { get; set; }
		public GameDest ExitRight { get; set; }
		public GameItem Item { get; set; }

		// local state

		public bool Explored { get; set; }

		public ZebesRoomState Clone()
		{
			var clone = new ZebesRoomState();

			clone.Ignored = this.Ignored;
			clone.ExitUp = this.ExitUp;
			clone.ExitDown = this.ExitDown;
			clone.ExitLeft = this.ExitLeft;
			clone.ExitRight = this.ExitRight;
			clone.Item = Item;

			return clone;
		}

		public void Mirror()
		{
			var dl = ExitLeft;
			var dr = ExitRight;

			ExitLeft = dr;
			ExitRight = dl;
		}
	}
}
