namespace MetalTracker.Games.Metroid.Types
{
	public class ZebesRoomProps
	{
		public bool Elevator { get; set; }

		public bool IsVertical { get; set; }

		public char SlotClass { get; set; } = '\0';

		public bool Shuffled { get; set; }

		public bool CanHaveDest()
		{
			return Elevator || IsVertical;
		}

		public bool CanHaveItem()
		{
			return SlotClass != '\0';
		}
	}
}
