using Eto.Drawing;

namespace MetalTracker.Common.Types
{
    public class GameItem
    {
        public string Game { get; private set; }

        public string Key { get; private set; }

        public string Name { get; private set; }

        public Image Icon { get; private set; }

        public GameItem(string game, string key, string name, Image icon)
        {
            Game = game;
            Key = key;
            Name = Name;
            Icon = icon;
        }

        public string GetCode()
        {
            return $"{Game}|{Key}";
        }
    }
}
