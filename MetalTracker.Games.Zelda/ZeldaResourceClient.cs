﻿using Eto.Drawing;
using MetalTracker.Common.Types;

namespace MetalTracker.Games.Zelda
{
	public static class ZeldaResourceClient
	{
		static GameExit[] GameExits = new GameExit[]
		{
			new GameExit("z1", "0", "OW", "Overworld"),
			new GameExit("z1", "1", "D1", "Dungeon #1"),
			new GameExit("z1", "2", "D2", "Dungeon #2"),
			new GameExit("z1", "3", "D3", "Dungeon #3"),
			new GameExit("z1", "4", "D4", "Dungeon #4"),
			new GameExit("z1", "5", "D5", "Dungeon #5"),
			new GameExit("z1", "6", "D6", "Dungeon #6"),
			new GameExit("z1", "7", "D7", "Dungeon #7"),
			new GameExit("z1", "8", "D8", "Dungeon #8"),
			new GameExit("z1", "9", "D9", "Dungeon #9"),
		};

		static GameItem[] GameItems = new GameItem[]
		{
			new GameItem("z1", "bombs", "Bombs", 'M', GetIcon("bomb")),
			new GameItem("z1", "key", "Key", 'M', GetIcon("key")),
			new GameItem("z1", "heart", "Heart", 'M', GetIcon("heart")),
			new GameItem("z1", "shield", "Big Shield", 'M', GetIcon("shield")),
			new GameItem("z1", "rupees", "Rupees", 'M', GetIcon("bluerupee")),
			new GameItem("z1", "potion1", "Blue Potion", 'M', GetIcon("bluepotion")),
			new GameItem("z1", "potion2", "Red Potion", 'M', GetIcon("redpotion")),
			new GameItem("z1", "map", "Dungeon Map", 'M', GetIcon("map")),
			new GameItem("z1", "compass", "Compass", 'M', GetIcon("compass")),
			new GameItem("z1", "tri", "Triforce Piece", 'Q', GetIcon("tri")),
			new GameItem("z1", "heartcontainer", "Heart Container", 'U', GetIcon("heartcontainer")),
			new GameItem("z1", "bombup", "Bomb Cap. Upgrade", 'U', GetIcon("bombsup")),
			new GameItem("z1", "bait", "Bait", 'E', GetIcon("bait1")),
			new GameItem("z1", "letter", "Letter", 'E', GetIcon("letter1")),
			new GameItem("z1", "raft", "Raft", 'E', GetIcon("raft1")),
			new GameItem("z1", "ladder", "Ladder", 'E', GetIcon("ladder1")),
			new GameItem("z1", "bracelet", "Bracelet", 'E', GetIcon("bracelet1")),
			new GameItem("z1", "recorder", "Recorder", 'E', GetIcon("recorder1")),
			new GameItem("z1", "magkey", "Magical Key", 'E', GetIcon("magkey1")),
			new GameItem("z1", "wand", "Wand", 'E', GetIcon("wand1")),
			new GameItem("z1", "book", "Book", 'E', GetIcon("book1")),
			new GameItem("z1", "ring1", "Blue Ring", 'E', GetIcon("ring1")),
			new GameItem("z1", "ring2", "Red Ring", 'E', GetIcon("ring2")),
			new GameItem("z1", "candle1", "Blue Candle", 'E', GetIcon("candle1")),
			new GameItem("z1", "candle2", "Red Candle", 'E', GetIcon("candle2")),
			new GameItem("z1", "boom1", "Wooden Boomerang", 'E', GetIcon("boom1")),
			new GameItem("z1", "boom2", "Magical Boomerang", 'E', GetIcon("boom2")),
			new GameItem("z1", "bow", "Bow", 'E', GetIcon("bow1")),
			new GameItem("z1", "arrow1", "Wooden Arrows", 'E', GetIcon("arrow1")),
			new GameItem("z1", "arrow2", "Silver Arrows", 'E', GetIcon("arrow2")),
			new GameItem("z1", "sword1", "Wooden Sword", 'E', GetIcon("sword1")),
			new GameItem("z1", "sword2", "White Sword", 'E', GetIcon("sword2")),
			new GameItem("z1", "sword3", "Magical Sword", 'E', GetIcon("sword3")),
		};

		public static GameExit[] GetGameExits()
		{
			return GameExits;
		}

		public static GameItem[] GetGameItems()
		{
			return GameItems;
		}

		public static Image GetIcon(string name)
		{
			return Bitmap.FromResource($"MetalTracker.Games.Zelda.Res.Icons.{name}.png", typeof(ZeldaResourceClient).Assembly);
		}
	}
}


