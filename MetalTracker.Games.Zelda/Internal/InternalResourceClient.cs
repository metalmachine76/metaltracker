using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eto.Drawing;
using MetalTracker.Games.Zelda.Types;

namespace MetalTracker.Games.Zelda
{
	public static class InternalResourceClient
	{
		static List<(bool, int, int, int)> DungeonItemBasements = new List<(bool, int, int, int)>
		{
			(false, 1, 1, 2),
			(false, 3, 0, 6),
			(false, 4, 2, 3),
			(false, 5, 1, 0),
			(false, 6, 1, 0),
			(false, 7, 2, 1),
			(false, 8, 1, 7),
			(false, 8, 4, 1),
			(false, 9, 0, 1),
			(false, 9, 7, 0),

			(true, 2, 1, 3),
			(true, 4, 2, 5),
			(true, 4, 3, 1),
			(true, 5, 2, 1),
			(true, 6, 0, 5),
			(true, 7, 2, 1),
			(true, 8, 0, 3),
			(true, 8, 0, 4),
			(true, 9, 0, 0),
			(true, 9, 5, 4),
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

			string resName = $"MetalTracker.Games.Zelda.Res.{q}.overworldmeta.{(q2 ? "q2" : "q1")}.txt";

			using (var str = typeof(InternalResourceClient).Assembly.GetManifestResourceStream(resName))
			{
				using (StreamReader sr = new StreamReader(str))
				{
					string metaString = sr.ReadToEnd();
					string[] lines = metaString.Split("\r\n");
					for (int y = 0; y < 8; y++)
					{
						string line = lines[y];
						for (int x = 0; x < 16; x++)
						{
							char c = line[x];
							meta[y, x] = new OverworldRoomProps(c == 'I', c == 'D');
						}
					}
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

		public static Image GetDungeonImage(bool q2, int level, bool mirrored)
		{
			Bitmap map;

			string q = q2 ? "q2" : "q1";
			string d = $"d{level}";

			string resName = $"MetalTracker.Games.Zelda.Res.{q}.{d}.{q}{(mirrored ? ".m" : "")}.png";

			map = Bitmap.FromResource(resName);

			return map;
		}

		public static OverworldRoomState[,] GetDefaultOverworldState(bool q2, bool dungeons, bool others, bool mirrored)
		{
			var caveDests = ZeldaResourceClient.GetCaveDestinations();
			var exitDests = ZeldaResourceClient.GetExitDestinations();
			var gameItems = ZeldaResourceClient.GetGameItems();

			OverworldRoomState[,] states = new OverworldRoomState[8, 16];

			string q = q2 ? "q2" : "q1";

			string resName = $"MetalTracker.Games.Zelda.Res.{q}.overworldcaves.txt";

			using (var str = typeof(InternalResourceClient).Assembly.GetManifestResourceStream(resName))
			{
				using (StreamReader sr = new StreamReader(str))
				{
					string metaString = sr.ReadToEnd();
					string[] lines = metaString.Split("\r\n");
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
									var caveDest = Array.Find(caveDests, d => d.Key == c.ToString());
									if (caveDest != null)
									{
										state.Destination = caveDest;
										if (caveDest.Key == "P")
										{
											state.Item1 = gameItems.First(i => i.Key == "potion1");
											state.Item3 = gameItems.First(i => i.Key == "potion2");
										}
									}
								}
								if (dungeons)
								{
									var exitDest = Array.Find(exitDests, d => d.Key == c.ToString());
									if (exitDest != null)
									{
										state.Destination = exitDest;
									}
								}
							}
							states[y, x] = state;
						}
					}
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

		public static DungeonRoomProps[,] GetDungeonMeta(bool q2, int level, int shuffleMode, bool mirrored)
		{
			string d = $"d{level}";
			string q = q2 ? "q2" : "q1";

			#region Load / Extract Meta and Shuffle Data

			string metaResName = $"MetalTracker.Games.Zelda.Res.{q}.{d}meta.txt";

			string metaString;

			using (var str = typeof(InternalResourceClient).Assembly.GetManifestResourceStream(metaResName))
			{
				using (StreamReader sr = new StreamReader(str))
				{
					metaString = sr.ReadToEnd();
				}
			}

			string[] metaLines = metaString.Split("\r\n");
			int w = metaLines[0].Length;

			string shuffleResName = $"MetalTracker.Games.Zelda.Res.{q}.{d}shuffle.txt";

			string shuffleString;

			using (var str = typeof(InternalResourceClient).Assembly.GetManifestResourceStream(shuffleResName))
			{
				using (StreamReader sr = new StreamReader(str))
				{
					shuffleString = sr.ReadToEnd();
				}
			}

			string[] shuffleLines = shuffleString.Split("\r\n");

			#endregion

			DungeonRoomProps[,] meta = new DungeonRoomProps[8, w];

			for (int y = 0; y < 8; y++)
			{
				string mline = metaLines[y];
				string sline = shuffleLines[y];
				for (int x = 0; x < w; x++)
				{
					char mc = mline[x];  // meta char
					char sc = sline[x];  // shuffle char

					DungeonRoomProps props;

					if (mc == '.')
					{
						props = new DungeonRoomProps(false, false, false, false, '\0', '\0', false);
					}
					else
					{
						// item slot 1 (floor item) class

						char s1c = '\0';

						if (mc == 'Q' || mc == 'E' || mc == 'U' || mc == 'M')
						{
							s1c = mc;
						}

						// possible exits

						bool destNorth = (y == 0) || metaLines[y - 1][x] == '.';
						bool destSouth = (y == 7) || metaLines[y + 1][x] == '.';
						bool destWest = (x == 0) || mline[x - 1] == '.';
						bool destEast = (x == w - 1) || mline[x + 1] == '.';

						if (level == 9)
						{
							destNorth = false;
							destWest = false;
							destEast = false;
						}

						bool lowerItem = DungeonItemBasements.Any(e => e.Item1 == q2 && e.Item2 == level && e.Item3 == x && e.Item4 == y);

						// if shuffle level is 2, room is shuffled
						// if shuffle level is 1, room is shuffled if minor

						bool shuffled = (shuffleMode == 2) || (shuffleMode == 1 && sc == 'm');

						props = new DungeonRoomProps(destNorth, destSouth, destWest, destEast, s1c, lowerItem ? 'E' : '\0', shuffled);
					}

					meta[y, x] = props;
				}
			}

			if (mirrored)
			{
				int m = w / 2;
				for (int y = 0; y < 8; y++)
				{
					for (int x = 0; x < m; x++)
					{
						var m0 = meta[y, x];
						var m1 = meta[y, (w - 1) - x];
						meta[y, x] = m1;
						meta[y, (w - 1) - x] = m0;
					}
				}
			}

			return meta;
		}
	}
}


