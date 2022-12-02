using Eto.Drawing;
using MetalTracker.Common.Types;

namespace MetalTracker.Games.Zelda
{
	public static class ZeldaResourceClient
	{
		public static GameDest[] GetCaveDestinations()
		{
			return new GameDest[]
			{
				new GameDest("z1", "S", "Shop", "Normal Shop", false, 3),
				new GameDest("z1", "P", "Potions", "Potions Shop", false, 3),
				new GameDest("z1", "I", "Free", "Free Item(s)", false, 3),
				new GameDest("z1", "SC", "W/M", "White/Magical Sword Cave", false, 1),
				new GameDest("z1", "G", "MMG", "Money Making Game"),
				new GameDest("z1", "R", "Road", "Take Any Road"),
				new GameDest("z1", "C", "Charge", "Door Repair Charge"),
				new GameDest("z1", "H", "Hint", "Hint"),
			};
		}

		public static GameDest[] GetExitDestinations()
		{
			return new GameDest[]
			{
				new GameDest("z1", "0", "OW", "Overworld", true),
				new GameDest("z1", "1", "D1", "Dungeon #1", true),
				new GameDest("z1", "2", "D2", "Dungeon #2", true),
				new GameDest("z1", "3", "D3", "Dungeon #3", true),
				new GameDest("z1", "4", "D4", "Dungeon #4", true),
				new GameDest("z1", "5", "D5", "Dungeon #5", true),
				new GameDest("z1", "6", "D6", "Dungeon #6", true),
				new GameDest("z1", "7", "D7", "Dungeon #7", true),
				new GameDest("z1", "8", "D8", "Dungeon #8", true),
				new GameDest("z1", "9", "D9", "Dungeon #9", true),
			};
		}

		public static GameItem[] GetGameItems()
		{
			return new GameItem[]
			{
				new GameItem("z1", "bombs", "Bombs", GetIcon("bomb")),
				new GameItem("z1", "key", "Key", GetIcon("key")),
				new GameItem("z1", "heart", "Heart", GetIcon("heart")),
				new GameItem("z1", "bait", "Bait", GetIcon("bait1")),
				new GameItem("z1", "shield", "Big Shield", GetIcon("shield")),
				new GameItem("z1", "rupees", "Rupees", GetIcon("bluerupee")),
				new GameItem("z1", "heartcontainer", "Heart Container", GetIcon("heartcontainer")),
				new GameItem("z1", "potion1", "Blue Potion", GetIcon("bluepotion")),
				new GameItem("z1", "potion2", "Red Potion", GetIcon("redpotion")),
				new GameItem("z1", "map", "Dungeon Map", GetIcon("map")),
				new GameItem("z1", "compass", "Compass", GetIcon("compass")),
				new GameItem("z1", "tri", "Triforce Piece", GetIcon("tri")),
				new GameItem("z1", "letter", "Letter", GetIcon("letter1")),
				new GameItem("z1", "raft", "Raft", GetIcon("raft1")),
				new GameItem("z1", "ladder", "Ladder", GetIcon("ladder1")),
				new GameItem("z1", "bracelet", "Bracelet", GetIcon("bracelet1")),
				new GameItem("z1", "recorder", "Recorder", GetIcon("recorder1")),
				new GameItem("z1", "magkey", "Magical Key", GetIcon("magkey1")),
				new GameItem("z1", "wand", "Wand", GetIcon("wand1")),
				new GameItem("z1", "ring1", "Blue Ring", GetIcon("ring1")),
				new GameItem("z1", "ring2", "Red Ring", GetIcon("ring2")),
				new GameItem("z1", "candle1", "Blue Candle", GetIcon("candle1")),
				new GameItem("z1", "candle2", "Red Candle", GetIcon("candle2")),
				new GameItem("z1", "boom1", "Wooden Boomerang", GetIcon("boom1")),
				new GameItem("z1", "boom2", "Magical Boomerang", GetIcon("boom2")),
				new GameItem("z1", "arrow1", "Wooden Arrows", GetIcon("arrow1")),
				new GameItem("z1", "arrow2", "Silver Arrows", GetIcon("arrow2")),
				new GameItem("z1", "sword1", "Wooden Sword", GetIcon("sword1")),
				new GameItem("z1", "sword2", "White Sword", GetIcon("sword2")),
				new GameItem("z1", "sword3", "Magical Sword", GetIcon("sword3")),
			};
		}

		public static Image GetIcon(string name)
		{
			return Bitmap.FromResource($"MetalTracker.Games.Zelda.Res.Icons.{name}.png");
		}
	}
}


