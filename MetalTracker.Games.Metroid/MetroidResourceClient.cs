using Eto.Drawing;
using MetalTracker.Common.Types;
namespace MetalTracker.Games.Metroid
{
	public static class MetroidResourceClient
	{
		static GameDest[] GameDestinations = new GameDest[]
		{
			new GameDest("m1", "B", "Brinstar", "Brinstar", true),
			new GameDest("m1", "N", "Norfair", "Norfair", true),
			new GameDest("m1", "K", "Kraid", "Kraid's Hideout", true),
			new GameDest("m1", "R", "Ridley", "Ridley's Hideout", true),
			new GameDest("m1", "T", "Tourian", "Tourian Bridge", true),
		};

		static GameItem[] GameItems = new GameItem[]
		{
				new GameItem("m1", "boots", "High Jump Boots", 'E', GetIcon("boots1")),
				new GameItem("m1", "ice", "Ice Beam", 'E', GetIcon("icebeam1")),
				new GameItem("m1", "wave", "Wave Beam", 'E', GetIcon("wavebeam1")),
				new GameItem("m1", "long", "Long Beam", 'E', GetIcon("longbeam1")),
				new GameItem("m1", "morph", "Morph Ball", 'E', GetIcon("morphball1")),
				new GameItem("m1", "bombs", "Morph Ball Bombs", 'E', GetIcon("morphbombs1")),
				new GameItem("m1", "screw", "Screw Attack", 'E', GetIcon("screwattack1")),
				new GameItem("m1", "varia", "Varia Suit", 'E', GetIcon("varia1")),
				new GameItem("m1", "kraidtot", "Totem of Kraid", 'Q', GetIcon("kraidtot1")),
				new GameItem("m1", "ridleytot", "Totem of Ridley", 'Q', GetIcon("ridleytot1")),
				new GameItem("m1", "missilepack", "Missile Pack", 'U', GetIcon("missilepack")),
				new GameItem("m1", "energytank", "Energy Tank", 'U', GetIcon("energytank")),
				new GameItem("m1", "missiles", "Missiles", 'M', GetIcon("missile")),
				new GameItem("m1", "energy", "Energy", 'M', GetIcon("energy")),
		};

		public static GameDest[] GetDestinations()
		{
			return GameDestinations;
		}

		public static GameItem[] GetGameItems()
		{
			return GameItems;
		}

		public static Image GetIcon(string name)
		{
			return Bitmap.FromResource($"MetalTracker.Games.Metroid.Res.Icons.{name}.png", typeof(MetroidResourceClient).Assembly);
		}
	}
}
