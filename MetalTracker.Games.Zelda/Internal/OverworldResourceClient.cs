using System;
using System.Linq;
using Eto.Drawing;
using MetalTracker.Common;
using MetalTracker.Games.Zelda.Internal.Types;

namespace MetalTracker.Games.Zelda.Internal
{
	internal static class OverworldResourceClient
	{
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
			var caveDests = ZeldaResourceClient.GetCaveDestinations();
			var exitDests = ZeldaResourceClient.GetExitDestinations();
			var gameItems = ZeldaResourceClient.GetGameItems();

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

		private static string[] GetResourceLines(string resName)
		{
			return ResourceClient.GetResourceLines(typeof(OverworldResourceClient).Assembly, resName);
		}
	}
}


