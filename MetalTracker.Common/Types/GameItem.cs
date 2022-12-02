using Eto.Drawing;

namespace MetalTracker.Common.Types
{
	public class GameItem
	{
		public string Game { get; private set; }

		public string Key { get; private set; }

		public string Name { get; private set; }

		public char Kind { get; private set; }

		public Image Icon { get; private set; }

		public GameItem(string game, string key, string name, char kind, Image icon)
		{
			this.Game = game;
			this.Key = key;
			this.Name = name;
			this.Kind = kind;
			this.Icon = icon;
		}

		public bool IsImportant()
		{
			return Kind == 'Q' || Kind == 'E';
		}

		public string GetCode()
		{
			return $"{Game}|{Key}";
		}
	}
}
