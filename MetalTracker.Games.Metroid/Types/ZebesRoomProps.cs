namespace MetalTracker.Games.Metroid.Types
{
	public class ZebesRoomProps
	{
		public bool Elevator { get; set; }

		public bool IsVertical { get; set; }

		public string SlotClass { get; set; }

		public bool CanHaveDest()
		{
			return Elevator || IsVertical;
		}

		public bool CanHaveItem()
		{
			return SlotClass != null;
		}
	}
}
