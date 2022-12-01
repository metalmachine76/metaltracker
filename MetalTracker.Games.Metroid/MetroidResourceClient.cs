using System.Collections.Generic;
using Eto.Drawing;
using MetalTracker.Common.Types;
namespace MetalTracker.Games.Metroid
{
	public static class MetroidResourceClient
	{
		public static GameDest[] GetDestinations()
		{
			return new GameDest[]
			{
				new GameDest("m1", "B", "Brinstar", "Brinstar", true),
				new GameDest("m1", "N", "Norfair", "Norfair", true),
				new GameDest("m1", "K", "Kraid", "Kraid's Hideout", true),
				new GameDest("m1", "R", "Ridley", "Ridley's Hideout", true),
				new GameDest("m1", "T", "Tourian", "Tourian Bridge", true),
			};
		}

		public static GameItem[] GetGameItems()
		{
			return new GameItem[]
			{
				new GameItem("m1", "boots", "High Jump Boots", GetIcon("boots1")),
				new GameItem("m1", "ice", "Ice Beam", GetIcon("icebeam1")),
				new GameItem("m1", "wave", "Wave Beam", GetIcon("wavebeam1")),
				new GameItem("m1", "long", "Long Beam", GetIcon("longbeam1")),
				new GameItem("m1", "morph", "Morph Ball", GetIcon("morphball1")),
				new GameItem("m1", "bombs", "Morph Ball Bombs", GetIcon("morphbombs1")),
				new GameItem("m1", "screw", "Screw Attack", GetIcon("screwattack1")),
				new GameItem("m1", "varia", "Varia Suit", GetIcon("varia1")),
				new GameItem("m1", "kraidtot", "Kraid Totem", GetIcon("kraidtot1")),
				new GameItem("m1", "ridleytot", "Ridley Totem", GetIcon("ridleytot1")),
				new GameItem("m1", "missilepack", "Missile Pack", GetIcon("missilepack")),
				new GameItem("m1", "energytank", "Energy Tank", GetIcon("energytank")),
				new GameItem("m1", "missiles", "Missiles", GetIcon("missile")),
				new GameItem("m1", "energy", "Energy", GetIcon("energy")),
			};
		}

		public static Image GetIcon(string name)
		{
			return Bitmap.FromResource($"MetalTracker.Games.Metroid.Res.Icons.{name}.png");
		}
	}
}
