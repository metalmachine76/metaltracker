namespace MetalTracker.Games.Metroid.Internal.Types
{
	internal class ZebesRoomProps
	{
		public bool ElevatorUp { get; set; }

		public bool ElevatorDown { get; set; }

		public bool CanExitLeft { get; set; }

		public bool CanExitRight { get; set; }

		public char SlotClass { get; set; } = '\0';

		public char AreaCode { get; set; } = '\0';

		public bool Shuffled { get; set; }

		public bool CanHaveDest()
		{
			return ElevatorUp || ElevatorDown || CanExitLeft || CanExitRight;
		}

		public bool CanHaveItem()
		{
			return SlotClass != '\0';
		}

		public void Mirror()
		{
			bool exl = this.CanExitLeft;
			bool exr = this.CanExitRight;
			this.CanExitLeft = exr;
			this.CanExitRight = exl;
		}
	}
}
