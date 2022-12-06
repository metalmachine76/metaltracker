namespace MetalTracker.Common.Types
{
	public class LocationOfItem
	{
		public GameItem Item { get; private set; }

		public string Location { get; private set; }

		public string Map { get; private set; }

		public int X { get; private set; }

		public int Y { get; private set; }

		public LocationOfItem(GameItem item, string location, string map, int x, int y)
		{
			this.Item = item;
			this.Location = location;
			this.Map = map;
			this.X = x;
			this.Y = y;
		}
	}
}
