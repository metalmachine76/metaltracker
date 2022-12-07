namespace MetalTracker.Games.Zelda.Types
{
	public class DungeonRoomProps
	{
		public bool DestNorth { get; private set; }
		public bool DestSouth { get; private set; }
		public bool DestWest { get; private set; }
		public bool DestEast { get; private set; }
		public char Slot1Class { get; private set; } = '\0';
		public char Slot2Class { get; private set; } = '\0';
		public bool HasStairs { get; private set; }
		public bool Shuffled { get; private set; }

		public DungeonRoomProps(bool destNorth, bool destSouth, bool destWest, bool destEast, char slot1Class, char slot2Class, bool stairs, bool shuffled)
		{
			this.DestNorth = destNorth;
			this.DestSouth = destSouth;
			this.DestWest = destWest;
			this.DestEast = destEast;
			this.Slot1Class = slot1Class;
			this.Slot2Class = slot2Class;
			this.HasStairs = stairs;
			this.Shuffled = shuffled;
		}

		public bool CanHaveDest()
		{
			return DestNorth || DestSouth || DestWest || DestEast;
		}

		public bool CanHaveItem1()
		{
			return Slot1Class != '\0';
		}

		public bool CanHaveItem2()
		{
			return Slot2Class != '\0';
		}

		public void Mirror()
		{
			bool e = this.DestEast;
			bool w = this.DestWest;
			this.DestEast = w;
			this.DestWest = e;
		}
	}
}
