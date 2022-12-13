namespace MetalTracker.Games.Zelda.Internal.Types
{
	internal class OverworldCave
	{
		public string Key { get; private set; }

		public string ShortName { get; private set; }

		public string LongName { get; private set; }

		public int ItemSlots { get; private set; }

		public OverworldCave(string key, string shortName, string longName, int itemSlots = 0)
		{
			this.Key = key;
			this.ShortName = shortName;
			this.LongName = longName;
			this.ItemSlots = itemSlots;
		}
	}
}
