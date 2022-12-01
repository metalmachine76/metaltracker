namespace MetalTracker.Games.Zelda.Types
{
	public class OverworldRoomProps
	{
		public bool ItemHere { get; private set; }

		public bool DestHere { get; private set; }

		public OverworldRoomProps(bool itemHere, bool destHere)
		{
			this.ItemHere = itemHere;
			this.DestHere = destHere;
		}
	}
}
