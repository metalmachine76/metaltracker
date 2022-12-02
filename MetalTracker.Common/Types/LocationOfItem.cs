namespace MetalTracker.Common.Types
{
	public class LocationOfItem
	{
		public GameItem Item { get; private set; }

		public string Location { get; private set; }

		public LocationOfItem(GameItem item, string location)
		{
			this.Item = item;
			this.Location = location;
		}
	}
}
