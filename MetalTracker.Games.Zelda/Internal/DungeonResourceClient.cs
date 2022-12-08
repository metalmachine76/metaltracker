using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eto.Drawing;
using MetalTracker.Games.Zelda.Internal.Types;

namespace MetalTracker.Games.Zelda.Internal
{
	internal static class DungeonResourceClient
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

		public static Image GetDungeonImage(bool q2, int level, bool mirrored)
		{
			Bitmap map;

			string q = q2 ? "q2" : "q1";
			string d = $"d{level}";

			string resName = $"MetalTracker.Games.Zelda.Res.{q}.{d}.{q}{(mirrored ? ".m" : "")}.png";

			map = Bitmap.FromResource(resName);

			return map;
		}

		public static DungeonRoomProps[,] GetDungeonMeta(bool q2, int level, int shuffleMode, bool mirrored)
		{
			string d = $"d{level}";
			string q = q2 ? "q2" : "q1";

			string[] metaLines = GetResourceLines($"MetalTracker.Games.Zelda.Res.{q}.{d}meta.txt");
			string[] shuffleLines = GetResourceLines($"MetalTracker.Games.Zelda.Res.{q}.{d}shuffle.txt");
			string[] stairsLines = GetResourceLines($"MetalTracker.Games.Zelda.Res.{q}.{d}stairs.txt");

			int w = metaLines[0].Length;

			DungeonRoomProps[,] meta = new DungeonRoomProps[8, w];

			for (int y = 0; y < 8; y++)
			{
				string mline = metaLines[y];
				string sline = shuffleLines[y];
				string tline = stairsLines[y];

				for (int x = 0; x < w; x++)
				{
					char mc = mline[x];  // meta char
					char sc = sline[x];  // shuffle char
					char tc = tline[x];  // stairs char

					DungeonRoomProps props;

					if (mc == '.')
					{
						props = null;
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

						bool stairs = tc != '.';

						props = new DungeonRoomProps(destNorth, destSouth, destWest, destEast, s1c, lowerItem ? 'E' : '\0', stairs, shuffled);
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
						var p0 = meta[y, x];
						var p1 = meta[y, (w - 1) - x];

						p0?.Mirror();
						p1?.Mirror();

						meta[y, x] = p1;
						meta[y, (w - 1) - x] = p0;
					}
				}
			}

			return meta;
		}

		public static DungeonRoomState[,] GetDefaultDungeonState(bool q2, int level, int shuffleMode, bool mirrored)
		{
			string d = $"d{level}";
			string q = q2 ? "q2" : "q1";

			string[] stairsLines = GetResourceLines($"MetalTracker.Games.Zelda.Res.{q}.{d}stairs.txt");

			int w = stairsLines[0].Length;

			DungeonRoomState[,] stateGrid = new DungeonRoomState[8, 8];

			for (int y = 0; y < 8; y++)
			{
				string tline = stairsLines[y];

				for (int x = 0; x < w; x++)
				{
					char tc = tline[x];  // stairs char

					DungeonRoomState state = new DungeonRoomState();

					if (tc != '.')
					{
						state.Transport = shuffleMode == 0 ? tc.ToString() : null;
					}

					stateGrid[y, x] = state;
				}
			}

			if (mirrored)
			{
				int m = w / 2;
				for (int y = 0; y < 8; y++)
				{
					for (int x = 0; x < m; x++)
					{
						var s0 = stateGrid[y, x];
						var s1 = stateGrid[y, (w - 1) - x];

						s0?.Mirror();
						s1?.Mirror();

						stateGrid[y, x] = s1;
						stateGrid[y, (w - 1) - x] = s0;
					}
				}
			}


			return stateGrid;
		}

		private static string[] GetResourceLines(string resName)
		{
			string resString;

			using (var str = typeof(DungeonResourceClient).Assembly.GetManifestResourceStream(resName))
			{
				using (StreamReader sr = new StreamReader(str))
				{
					resString = sr.ReadToEnd();
				}
			}

			string[] resLines = resString.Split("\r\n");

			return resLines;
		}
	}
}


