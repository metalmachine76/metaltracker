using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using MetalTracker.Common.Controls;
using MetalTracker.Games.Metroid;
using MetalTracker.Games.Zelda;
using MetalTracker.Trackers.Z1M1.Internal;

namespace MetalTracker.Trackers.Z1M1.Proxies
{
	internal class ItemTracker
	{
		private readonly Panel _panel;
		private readonly List<TrackedItemView> _trackedItemViews;

		public ItemTracker(Panel panel)
		{
			_panel = panel;
			_trackedItemViews = new List<TrackedItemView>();
		}

		public void Init()
		{
			var mainLayout = new StackLayout { Orientation = Orientation.Vertical, HorizontalContentAlignment = HorizontalAlignment.Center };

			// zelda rows

			var zeldaRow0 = new StackLayout { Orientation = Orientation.Horizontal, Padding = new Padding(5, 5) };
			var zeldaRow1 = new StackLayout { Orientation = Orientation.Horizontal, Padding = new Padding(5, 5) };
			var zeldaRow2 = new StackLayout { Orientation = Orientation.Horizontal, Padding = new Padding(5, 5) };

			AddZeldaTrackedItem(zeldaRow0, "z_sword", "sword0", "sword1", "sword2", "sword3");
			AddZeldaTrackedItem(zeldaRow0, "z_bow", "bow0", "bow1");
			AddZeldaTrackedItem(zeldaRow0, "z_arrow", "arrow0", "arrow1", "arrow2");
			AddZeldaTrackedItem(zeldaRow0, "z_raft", "raft0", "raft1");
			AddZeldaTrackedItem(zeldaRow0, "z_recorder", "recorder0", "recorder1");
			AddZeldaTrackedItem(zeldaRow0, "z_ladder", "ladder0", "ladder1");
			AddZeldaTrackedItem(zeldaRow0, "z_candle", "candle0", "candle1", "candle2");
			AddZeldaTrackedItem(zeldaRow0, "z_letter", "letter0", "letter1");

			AddZeldaTrackedItem(zeldaRow1, "z_bracelet", "bracelet0", "bracelet1");
			AddZeldaTrackedItem(zeldaRow1, "z_bait", "bait0", "bait1");
			AddZeldaTrackedItem(zeldaRow1, "z_boom", "boom0", "boom1", "boom2");
			AddZeldaTrackedItem(zeldaRow1, "z_ring", "ring0", "ring1", "ring2");
			AddZeldaTrackedItem(zeldaRow1, "z_wand", "wand0", "wand1");
			AddZeldaTrackedItem(zeldaRow1, "z_book", "book0", "book1");
			AddZeldaTrackedItem(zeldaRow1, "z_magkey", "magkey0", "magkey1");

			AddZeldaTrackedItem(zeldaRow2, "z_d1tr", "d1tri0", "d1tri1");
			AddZeldaTrackedItem(zeldaRow2, "z_d2tr", "d2tri0", "d2tri1");
			AddZeldaTrackedItem(zeldaRow2, "z_d3tr", "d3tri0", "d3tri1");
			AddZeldaTrackedItem(zeldaRow2, "z_d4tr", "d4tri0", "d4tri1");
			AddZeldaTrackedItem(zeldaRow2, "z_d5tr", "d5tri0", "d5tri1");
			AddZeldaTrackedItem(zeldaRow2, "z_d6tr", "d6tri0", "d6tri1");
			AddZeldaTrackedItem(zeldaRow2, "z_d7tr", "d7tri0", "d7tri1");
			AddZeldaTrackedItem(zeldaRow2, "z_d8tr", "d8tri0", "d8tri1");

			mainLayout.Items.Add(zeldaRow0);
			mainLayout.Items.Add(zeldaRow1);
			mainLayout.Items.Add(zeldaRow2);

			// metroid rows

			var metroidRow0 = new StackLayout { Orientation = Orientation.Horizontal, Padding = new Padding(5, 5) };
			var metroidRow1 = new StackLayout { Orientation = Orientation.Horizontal, Padding = new Padding(5, 5) };

			AddMetroidTrackedItem(metroidRow0, "m_morph", "morphball0", "morphball1");
			AddMetroidTrackedItem(metroidRow0, "m_bombs", "morphbombs0", "morphbombs1");
			AddMetroidTrackedItem(metroidRow0, "m_varia", "varia0", "varia1");
			AddMetroidTrackedItem(metroidRow0, "m_boots", "boots0", "boots1");
			AddMetroidTrackedItem(metroidRow0, "m_screw", "screwattack0", "screwattack1");
			AddMetroidTrackedItem(metroidRow0, "m_long", "longbeam0", "longbeam1");
			AddMetroidTrackedItem(metroidRow0, "m_ice", "icebeam0", "icebeam1");
			AddMetroidTrackedItem(metroidRow0, "m_wave", "wavebeam0", "wavebeam1");

			AddMetroidTrackedItem(metroidRow1, "m_kraid", "kraidtot0", "kraidtot1");
			AddMetroidTrackedItem(metroidRow1, "m_ridley", "ridleytot0", "ridleytot1");

			mainLayout.Items.Add(metroidRow0);
			mainLayout.Items.Add(metroidRow1);

			_panel.Content = mainLayout;
		}

		public void SetInventory(List<InventoryEntry> entries)
		{
			foreach (var view in _trackedItemViews)
			{
				var entry = entries.Find(e => e.Key == view.ItemKey);
				if (entry != null)
				{
					view.ItemLevel = entry.Level;
				}
				else
				{
					view.ItemLevel = 0;
				}
				view.Invalidate();
			}
		}

		public List<InventoryEntry> GetInventory()
		{
			List<InventoryEntry> entries = new List<InventoryEntry>();

			foreach (var view in _trackedItemViews)
			{
				if (view.ItemLevel > 0)
				{
					entries.Add(new InventoryEntry { Key = view.ItemKey, Level = view.ItemLevel });
				}
			}

			return entries;
		}

		private void AddZeldaTrackedItem(StackLayout row, string key, params string[] iconNames)
		{
			int count = iconNames.Length;

			Image[] images = new Image[count];

			for (int i = 0; i < count; i++)
			{
				images[i] = ZeldaResourceClient.GetIcon(iconNames[i]);
			}

			var itemView = new TrackedItemView(key, images);

			_trackedItemViews.Add(itemView);

			row.Items.Add(itemView);
		}

		private void AddMetroidTrackedItem(StackLayout row, string key, params string[] iconNames)
		{
			int count = iconNames.Length;

			Image[] images = new Image[count];

			for (int i = 0; i < count; i++)
			{
				images[i] = MetroidResourceClient.GetIcon(iconNames[i]);
			}

			var itemView = new TrackedItemView(key, images);

			_trackedItemViews.Add(itemView);

			row.Items.Add(itemView);
		}
	}
}
