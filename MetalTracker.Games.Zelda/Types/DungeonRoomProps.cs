namespace MetalTracker.Games.Zelda.Types
{
	public class DungeonRoomProps
	{
		public bool DestNorth { get; private set; }
		public bool DestSouth { get; private set; }
		public bool DestWest { get; private set; }
		public bool DestEast { get; private set; }
		public char SlotClass { get; private set; } = '\0';

		public DungeonRoomProps(bool destNorth, bool destSouth, bool destWest, bool destEast, char slotClass)
		{
			this.DestNorth = destNorth;
			this.DestSouth = destSouth;
			this.DestWest = destWest;
			this.DestEast = destEast;
			this.SlotClass = slotClass;
		}

		public bool CanHaveDest()
		{
			return DestNorth || DestSouth || DestWest || DestEast;
		}

		public bool CanHaveItem()
		{
			return SlotClass != '\0';
		}
	}
}
