namespace MetalTracker.Common.Types
{
	public class LocationOfExit
	{
		public GameExit Dest { get; private set; }

		public string Location { get; private set; }

		public string Map { get; private set; }

		public int X { get; private set; }

		public int Y { get; private set; }

		public LocationOfExit(GameExit dest, string location, string map, int x, int y)
		{
			this.Dest = dest;
			this.Location = location;
			this.Map = map;
			this.X = x;
			this.Y = y;
		}
	}
}
