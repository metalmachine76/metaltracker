namespace MetalTracker.Common.Types
{
	public class GameExit
	{
		public string Game { get; private set; }

		public string Key { get; private set; }

		public string ShortName { get; private set; }

		public string LongName { get; private set; }

		public GameExit(string game, string key, string shortName, string longName)
		{
			Game = game;
			Key = key;
			ShortName = shortName;
			LongName = longName;
		}

		public string GetCode()
		{
			return $"{Game}|{Key}";
		}
	}
}