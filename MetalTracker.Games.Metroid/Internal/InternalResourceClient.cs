using System.Collections.Generic;
using System.IO;
using Eto.Drawing;
using MetalTracker.Games.Metroid.Types;

namespace MetalTracker.Games.Metroid.Internal
{
	public static class InternalResourceClient
	{
		static List<(int, int, char)> ItemSlots = new List<(int, int, char)>
		{
			(0x0F,0x02,'E'),
			(0x18,0x03,'U'),
			(0x1B,0x03,'U'),
			(0x07,0x05,'E'),
			(0x19,0x05,'E'),
			(0x19,0x07,'U'),
			(0x0F,0x09,'M'),
			(0x13,0x09,'E'),
			(0x1B,0x0A,'U'),
			(0x1C,0x0A,'U'),
			(0x12,0x0B,'U'),
			(0x1A,0x0B,'U'),
			(0x1B,0x0B,'U'),
			(0x1C,0x0B,'U'),
			(0x1A,0x0C,'E'),
			(0x02,0x0E,'E'),
			(0x09,0x0E,'U'),
			(0x12,0x0E,'U'),
			(0x1B,0x0E,'M'),
			(0x11,0x0F,'U'),
			(0x13,0x0F,'U'),
			(0x14,0x0F,'M'),
			(0x0F,0x10,'E'),
			(0x09,0x11,'M'),
			(0x1B,0x11,'E'),
			(0x09,0x12,'M'),
			(0x09,0x13,'M'),
			(0x1A,0x13,'U'),
			(0x1C,0x14,'U'),
			(0x04,0x15,'U'),
			(0x09,0x15,'U'),
			(0x12,0x15,'E'),
			(0x0A,0x16,'U'),
			(0x13,0x16,'U'),
			(0x14,0x16,'U'),
			(0x05,0x18,'M'),
			(0x12,0x18,'U'),
			(0x0A,0x19,'U'),
			(0x0E,0x19,'M'),
			(0x11,0x19,'U'),
			(0x12,0x19,'M'),
			(0x13,0x19,'M'),
			(0x14,0x19,'M'),
			(0x15,0x19,'M'),
			(0x05,0x1A,'M'),
			(0x05,0x1B,'U'),
			(0x0E,0x1B,'M'),
			(0x18,0x1B,'U'),
			(0x19,0x1B,'U'),
			(0x03,0x1C,'M'),
			(0x05,0x1C,'M'),
			(0x07,0x1C,'U'),
			(0x07,0x1D,'Q'),
			(0x0F,0x1D,'Q'),
			(0x0E,0x1E,'M'),
			(0x14,0x1E,'U'),
		};

		public static Image GetZebesImage(bool mirrored)
		{
			if (mirrored)
				return Bitmap.FromResource("MetalTracker.Games.Metroid.Res.zebesmap.m.png");
			else
				return Bitmap.FromResource("MetalTracker.Games.Metroid.Res.zebesmap.png");
		}

		public static ZebesRoomProps[,] GetZebesMeta(bool mirrored)
		{
			ZebesRoomProps[,] meta = new ZebesRoomProps[32, 32];

			string resName = $"MetalTracker.Games.Metroid.Res.zebesmeta.txt";

			using (var str = typeof(InternalResourceClient).Assembly.GetManifestResourceStream(resName))
			{
				using (StreamReader sr = new StreamReader(str))
				{
					string metaString = sr.ReadToEnd();
					string[] lines = metaString.Split("\r\n");
					for (int y = 0; y < 32; y++)
					{
						string line = lines[y];
						for (int x = 0; x < 32; x++)
						{
							char c = line[x];
							var props = new ZebesRoomProps();

							props.IsVertical = (c == '|');
							props.Elevator = (c == 'U' || c == 'D');

							meta[y, x] = props;
						}
					}
				}
			}

			foreach (var p in ItemSlots)
			{
				meta[p.Item2, p.Item1].SlotClass = p.Item3;
			}

			if (mirrored)
			{
				for (int y = 0; y < 32; y++)
				{
					for (int x = 0; x < 16; x++)
					{
						var m0 = meta[y, x];
						var m1 = meta[y, 31 - x];
						meta[y, x] = m1;
						meta[y, 31 - x] = m0;
					}
				}
			}

			return meta;
		}
	}
}
