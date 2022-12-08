using MetalTracker.Common.Types;

namespace MetalTracker.Games.Zelda.Internal.Types
{
    internal class DungeonRoomState
    {
        // shared state

        public GameDest DestNorth { get; set; }
        public GameDest DestSouth { get; set; }
        public GameDest DestWest { get; set; }
        public GameDest DestEast { get; set; }
        public DungeonWallType? WallNorth { get; set; }
        public DungeonWallType? WallSouth { get; set; }
        public DungeonWallType? WallWest { get; set; }
        public DungeonWallType? WallEast { get; set; }
        public GameItem Item1 { get; set; }
        public GameItem Item2 { get; set; }
        public string Transport { get; set; }

        // local state

        public bool Explored { get; set; }

        public DungeonRoomState Clone()
        {
            var clone = new DungeonRoomState();

            clone.DestNorth = DestNorth;
            clone.DestSouth = DestSouth;
            clone.DestWest = DestWest;
            clone.DestEast = DestEast;
            clone.Item1 = Item1;
            clone.Item2 = Item2;
            clone.Transport = Transport;

            return clone;
        }

        public void Mirror()
        {
            var e = DestEast;
            var w = DestWest;
            DestEast = w;
            DestWest = e;
        }
    }
}
