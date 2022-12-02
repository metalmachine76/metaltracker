namespace MetalTracker.Common.Types
{
	public class LocationOfDest
	{
		public GameDest Dest { get; private set; }

		public string Location { get; private set; }

		public LocationOfDest(GameDest dest, string location)
		{
			this.Dest = dest;
			this.Location = location;
		}
	}
}
