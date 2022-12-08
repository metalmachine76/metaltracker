namespace MetalTracker.CoOp.Interface
{
	public class FoundEventArgs
	{
		public string PlayerId { get; private set; }

		public string Type { get; private set; }

		public string Game { get; private set; }

		public string Map { get; private set; }

		public int X { get; private set; }

		public int Y { get; private set; }

		public int Slot { get; private set; }

		public string Code { get; private set; }

		public FoundEventArgs(string playerId, string type, string game, string map, int x, int y, int slot, string code)
		{
			this.PlayerId = playerId;
			this.Type = type;
			this.Game = game;
			this.Map = map;
			this.X = x;
			this.Y = y;
			this.Slot = slot;
			this.Code = code;
		}
	}
}
