namespace MetalTracker.Common.Types
{
	public class LocationOfItem
	{
		public GameItem Item { get; private set; }

		public string Location { get; private set; }

		public int X { get; private set; }

		public int Y { get; private set; }

		public LocationOfItem(GameItem item, string location, int x, int y)
		{
			this.Item = item;
			this.Location = location;
			this.X = x;
			this.Y = y;
		}
	}
}
