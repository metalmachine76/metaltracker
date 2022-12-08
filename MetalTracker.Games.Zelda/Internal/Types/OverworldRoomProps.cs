namespace MetalTracker.Games.Zelda.Internal.Types
{
    internal class OverworldRoomProps
    {
        public bool ItemHere { get; private set; }

        public bool DestHere { get; private set; }

        public OverworldRoomProps(bool itemHere, bool destHere)
        {
            ItemHere = itemHere;
            DestHere = destHere;
        }
    }
}
