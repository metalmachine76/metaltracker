using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using MetalTracker.Common;
using MetalTracker.Games.Zelda.Internal.Types;

namespace MetalTracker.Games.Zelda.Internal
{
	internal static class OverworldResourceClient
	{
		static List<OverworldCave> Caves = new List<OverworldCave>()
		{
			new OverworldCave("S", "Shop", "Normal Shop", 3),
			new OverworldCave("P", "Potions", "Potions Shop", 3),
			new OverworldCave("I", "Free", "Free Item(s)", 3),
			new OverworldCave("W", "W/M", "White/Magical Sword Cave", 1),
			new OverworldCave("G", "MMG", "Money Making Game"),
			new OverworldCave("R", "Road", "Take Any Road"),
			new OverworldCave("C", "Charge", "Door Repair Charge"),
			new OverworldCave("H", "Hint", "Hint"),
		};

		public static Image GetOverworldImage(bool q2, bool mirrored)
		{
			Bitmap map;

			string q = q2 ? "q2" : "q1";

			string resName = $"MetalTracker.Games.Zelda.Res.{q}.overworld.{q}{(mirrored ? ".m" : "")}.png";

			map = Bitmap.FromResource(resName);

			return map;
		}

		public static OverworldRoomProps[,] GetOverworldMeta(bool q2, bool mirrored)
		{
			OverworldRoomProps[,] meta = new OverworldRoomProps[8, 16];

			string q = q2 ? "q2" : "q1";

			string[] lines = GetResourceLines($"MetalTracker.Games.Zelda.Res.{q}.overworldmeta.{(q2 ? "q2" : "q1")}.txt");

			for (int y = 0; y < 8; y++)
			{
				string line = lines[y];
				for (int x = 0; x < 16; x++)
				{
					char c = line[x];
					meta[y, x] = new OverworldRoomProps(c == 'I', c == 'D');
				}
			}

			if (mirrored)
			{
				for (int y = 0; y < 8; y++)
				{
					for (int x = 0; x < 8; x++)
					{
						var m0 = meta[y, x];
						var m1 = meta[y, 15 - x];
						meta[y, x] = m1;
						meta[y, 15 - x] = m0;
					}
				}
			}

			return meta;
		}

		public static OverworldRoomState[,] GetDefaultOverworldState(bool q2, bool dungeons, bool others, bool mirrored)
		{
			var zeldaExits = ZeldaResourceClient.GetGameExits();
			var zeldaItems = ZeldaResourceClient.GetGameItems();

			OverworldRoomState[,] states = new OverworldRoomState[8, 16];

			string q = q2 ? "q2" : "q1";

			string[] lines = GetResourceLines($"MetalTracker.Games.Zelda.Res.{q}.overworldcaves.txt");

			for (int y = 0; y < 8; y++)
			{
				string line = lines[y];
				for (int x = 0; x < 16; x++)
				{
					char c = line[x];
					var state = new OverworldRoomState();
					if (c != '.')
					{
						if (others)
						{
							var cave = Caves.Find(d => d.Key == c.ToString());
							if (cave != null)
							{
								state.Cave = cave;
								if (cave.Key == "P")
								{
									state.Item1 = zeldaItems.First(i => i.Key == "potion1");
									state.Item3 = zeldaItems.First(i => i.Key == "potion2");
								}
							}
						}
						if (dungeons)
						{
							var exitDest = Array.Find(zeldaExits, d => d.Key == c.ToString());
							if (exitDest != null)
							{
								state.Exit = exitDest;
							}
						}
					}
					states[y, x] = state;
				}
			}

			if (mirrored)
			{
				for (int y = 0; y < 8; y++)
				{
					for (int x = 0; x < 8; x++)
					{
						var s0 = states[y, x];
						var s1 = states[y, 15 - x];
						states[y, x] = s1;
						states[y, 15 - x] = s0;
					}
				}
			}

			return states;
		}

		public static IReadOnlyList<OverworldCave> GetCaves()
		{
			return Caves;
		}

		private static string[] GetResourceLines(string resName)
		{
			return ResourceClient.GetResourceLines(typeof(OverworldResourceClient).Assembly, resName);
		}
	}
}


