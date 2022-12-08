namespace MetalTracker.Games.Zelda.Internal.Types
{
    internal class DungeonRoomProps
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
            DestNorth = destNorth;
            DestSouth = destSouth;
            DestWest = destWest;
            DestEast = destEast;
            Slot1Class = slot1Class;
            Slot2Class = slot2Class;
            HasStairs = stairs;
            Shuffled = shuffled;
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
            bool e = DestEast;
            bool w = DestWest;
            DestEast = w;
            DestWest = e;
        }
    }
}
