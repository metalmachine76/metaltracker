using MetalTracker.Common.Types;

namespace MetalTracker.Games.Metroid.Internal.Types
{
	internal class ZebesRoomState
	{
		// shared state

		public bool Ignored { get; set; }
		public GameExit ExitUp { get; set; }
		public GameExit ExitDown { get; set; }
		public GameExit ExitLeft { get; set; }
		public GameExit ExitRight { get; set; }
		public GameItem Item { get; set; }

		// local state

		/// <summary>
		/// 0 = none
		/// 1 = explored
		/// 2 = ignored
		/// </summary>
		public int Status { get; set; }

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
