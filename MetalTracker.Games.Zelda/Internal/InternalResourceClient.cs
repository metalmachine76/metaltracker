using System.IO;
using Eto.Drawing;
using MetalTracker.Games.Zelda.Types;

namespace MetalTracker.Games.Zelda
{
	public static class InternalResourceClient
	{
		public static Image GetOverworldImage(bool q2, bool mirrored)
		{
			Bitmap map;

			string resName = $"MetalTracker.Games.Zelda.Res.overworld.{(q2 ? "q2" : "q1")}{(mirrored ? ".m" : "")}.png";

			map = Bitmap.FromResource(resName);

			return map;
		}

		public static OverworldRoomProps[,] GetOverworldMeta(bool q2, bool mirrored)
		{
			OverworldRoomProps[,] meta = new OverworldRoomProps[8, 16];

			string resName = $"MetalTracker.Games.Zelda.Res.overworldmeta.{(q2 ? "q2" : "q1")}.txt";

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
	}
}


