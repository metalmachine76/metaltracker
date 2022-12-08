using MetalTracker.Common.Types;

namespace MetalTracker.Games.Metroid.Internal.Types
{
    internal class ZebesRoomState
    {
        // shared state

        public GameDest DestElev { get; set; }
        //public GameDest DestLeft { get; set; }
        //public GameDest DestRight { get; set; }
        public GameItem Item { get; set; }

        // local state

        public bool Explored { get; set; }

        public ZebesRoomState Clone()
        {
            var clone = new ZebesRoomState();

            clone.DestElev = DestElev;
            //clone.DestLeft = this.DestLeft;
            //clone.DestRight = this.DestRight;
            clone.Item = Item;

            return clone;
        }
    }
}
