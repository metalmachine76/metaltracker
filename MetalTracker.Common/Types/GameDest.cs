namespace MetalTracker.Common.Types
{
	public class GameDest
	{
		public string Game { get; private set; }

		public string Key { get; private set; }

		public string ShortName { get; private set; }

		public string LongName { get; private set; }

		public bool IsExit { get; private set; }

		public int ItemSlots { get; private set; }

		public GameDest(string game, string key, string shortName, string longName, bool isExit = false, int itemSlots = 0)
		{
			Game = game;
			Key = key;
			ShortName = shortName;
			LongName = longName;
			IsExit = isExit;
			ItemSlots = itemSlots;
		}

		public string GetCode()
		{
			return $"{Game}|{Key}";
		}
	}
}