using MetalTracker.Common.Types;

namespace MetalTracker.Games.Zelda.Internal.Types
{
    internal class OverworldRoomState
    {
        // shared state

        public GameDest Destination { get; set; }
        public GameItem Item1 { get; set; }
        public GameItem Item2 { get; set; }
        public GameItem Item3 { get; set; }

        // player state

        public bool Explored { get; set; }

        public OverworldRoomState Clone()
        {
            var clone = new OverworldRoomState();

            clone.Destination = Destination;
            clone.Item1 = Item1;
            clone.Item2 = Item2;
            clone.Item3 = Item3;

            return clone;
        }
    }
}
