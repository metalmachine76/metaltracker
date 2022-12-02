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
			(false, 3, 0, 6),
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

		public static DungeonRoomProps[,] GetDungeonMeta(bool q2, int level, bool mirrored)
		{
			string d = $"d{level}";
			string q = q2 ? "q2" : "q1";

			string resName = $"MetalTracker.Games.Zelda.Res.{q}.{d}meta.txt";

			string metaString;

			using (var str = typeof(InternalResourceClient).Assembly.GetManifestResourceStream(resName))
			{
				using (StreamReader sr = new StreamReader(str))
				{
					metaString = sr.ReadToEnd();
				}
			}

			string[] lines = metaString.Split("\r\n");
			int w = lines[0].Length;

			DungeonRoomProps[,] meta = new DungeonRoomProps[8, w];

			for (int y = 0; y < 8; y++)
			{
				string line = lines[y];
				for (int x = 0; x < w; x++)
				{
					char c = line[x];

					DungeonRoomProps props;

					if (c == '.')
					{
						props = new DungeonRoomProps(false, false, false, false, '\0');
					}
					else
					{
						char s1c = '\0';

						if (c == 'Q' || c == 'E' || c == 'U' || c == 'M')
						{
							s1c = c;
						}

						bool destNorth = (y == 0) || lines[y - 1][x] == '.';
						bool destSouth = (y == 7) || lines[y + 1][x] == '.';
						bool destWest = (x == 0) || line[x - 1] == '.';
						bool destEast = (x == w - 1) || line[x + 1] == '.';

						if (level == 9)
						{
							destNorth = false;
							destWest = false;
							destEast = false;
						}

						bool lowerItem = DungeonItemBasements.Any(e => e.Item1 == q2 && e.Item2 == level && e.Item3 == x && e.Item4 == y);

						props = new DungeonRoomProps(destNorth, destSouth, destWest, destEast, s1c, lowerItem ? 'E' : '\0');
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


