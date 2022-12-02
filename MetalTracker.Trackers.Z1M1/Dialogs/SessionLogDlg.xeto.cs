using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Serialization.Xaml;
using MetalTracker.Common.Bases;

namespace MetalTracker.Trackers.Z1M1.Dialogs
{
	public class SessionLogDlg : Dialog
	{
		class SessionLogEntry
		{
			public string Name { get; set; }
			public string Location { get; set; }
		}

		private List<BaseMap> _maps = new List<BaseMap>();

		public SessionLogDlg()
		{
			XamlReader.Load(this);
		}

		public void AddMap(BaseMap map)
		{
			_maps.Add(map);
		}

		protected void HandleLoad(object sender, EventArgs e)
		{
			GridView gridView = this.FindChild<GridView>("gridViewSessionLog");

			gridView.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell { Binding = Binding.Property<SessionLogEntry, string>(r => r.Name) },
				HeaderText = "Name",
				Width = 150
			});

			gridView.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell { Binding = Binding.Property<SessionLogEntry, string>(r => r.Location) },
				HeaderText = "Location",
				Width = 200
			});

			Show("items");
		}

		protected void HandleRadioShown(object sender, EventArgs e)
		{
			RadioButtonList rbl = sender as RadioButtonList;
			rbl.SelectedKey = "items";
		}

		protected void HandleRadioChanged(object sender, EventArgs e)
		{
			RadioButtonList rbl = sender as RadioButtonList;
			Show(rbl.SelectedKey);
		}

		private void Show(string key)
		{
			List<SessionLogEntry> entries = new List<SessionLogEntry>();

			if (key == "items")
			{
				foreach (var map in _maps)
				{
					foreach (var loc in map.GetItemLocations())
					{
						entries.Add(new SessionLogEntry { Name = loc.Item.Name, Location = loc.Location });
					}
				}
			}
			if (key == "exits")
			{
				foreach (var map in _maps)
				{
					foreach (var loc in map.GetExitLocations())
					{
						entries.Add(new SessionLogEntry { Name = loc.Dest.LongName, Location = loc.Location });
					}
				}
			}

			GridView gridView = this.FindChild<GridView>("gridViewSessionLog");
			gridView.DataStore = entries;
		}
	}
}
