namespace MetalTracker.Common.Types
{
	public class LocationOfDest
	{
		public GameDest dest { get; private set; }

		public string Location { get; private set; }

		public int X { get; private set; }

		public int Y { get; private set; }

		public LocationOfDest(GameDest dest, string location, int x, int y)
		{
			this.dest = dest;
			this.Location = location;
			this.X = x;
			this.Y = y;
		}
	}
}
