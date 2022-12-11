using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Eto.Forms;
using Eto.Serialization.Xaml;
using MetalTracker.Common.Bases;
using MetalTracker.Common.Types;
using MetalTracker.CoOp;
using MetalTracker.Games.Metroid;
using MetalTracker.Games.Metroid.Proxies;
using MetalTracker.Games.Zelda;
using MetalTracker.Games.Zelda.Proxies;
using MetalTracker.Trackers.Z1M1.Dialogs;
using MetalTracker.Trackers.Z1M1.Forms;
using MetalTracker.Trackers.Z1M1.Internal;
using MetalTracker.Trackers.Z1M1.Proxies;

namespace MetalTracker.Trackers.Z1M1
{
	public class MainForm : Form
	{
		private readonly OverworldMap _overworldMap = null;
		private readonly DungeonMap[] _dungeonMaps = new DungeonMap[9];
		private readonly ZebesMap _zebesMap = null;

		private readonly BaseMap[] _gameMaps = new BaseMap[11];

		private readonly ItemTracker _itemTracker = null;

		private SessionFlags _sessionFlags = new SessionFlags();
		private string _sessionFilename = null;

		private CoOpConfig _coOpConfig;
		private CoOpClient _coOpClient;

		public MainForm()
		{
			XamlReader.Load(this);

			List<GameItem> gameItems = new List<GameItem>();

			gameItems.AddRange(ZeldaResourceClient.GetGameItems());
			gameItems.AddRange(MetroidResourceClient.GetGameItems());

			var zeldaCaveDests = ZeldaResourceClient.GetCaveDestinations();
			var zeldaExitDests = ZeldaResourceClient.GetExitDestinations();
			var zebesExitDests = MetroidResourceClient.GetDestinations();

			var drawableCurrentMap = this.FindChild<Drawable>("drawableCurrentMap");
			var roomDetailContainer = this.FindChild<GroupBox>("groupBoxRoomDetail");

			_overworldMap = new OverworldMap(drawableCurrentMap, roomDetailContainer);
			_overworldMap.AddDestinations(zeldaCaveDests);
			_overworldMap.AddDestinations(zeldaExitDests.Where(d => d.Key != "0"));
			_overworldMap.AddDestinations(zebesExitDests);
			_overworldMap.SetGameItems(gameItems);

			_gameMaps[0] = _overworldMap;

			for (int i = 0; i < 9; i++)
			{
				var dungeonMap = new DungeonMap(drawableCurrentMap, roomDetailContainer);
				dungeonMap.AddDestinations(zeldaExitDests);
				dungeonMap.AddDestinations(zebesExitDests);
				dungeonMap.SetGameItems(gameItems);
				_dungeonMaps[i] = dungeonMap;
				_gameMaps[i + 1] = dungeonMap;
			}

			_zebesMap = new ZebesMap(drawableCurrentMap, roomDetailContainer);
			_zebesMap.AddDestinations(zeldaExitDests);
			_zebesMap.AddDestinations(zebesExitDests);
			_zebesMap.SetGameItems(gameItems);
			_gameMaps[10] = _zebesMap;

			var itemTrackerContainer = this.FindChild<GroupBox>("groupBoxItemTracker");
			_itemTracker = new ItemTracker(itemTrackerContainer);
		}

		public void LocateGoal(BaseMap map, int x, int y)
		{
			for (int i = 0; i < 11; i++)
			{
				if (map == _gameMaps[i])
				{
					DropDown dropDown = this.FindChild<DropDown>("dropDownSelectedMap");
					dropDown.SelectedIndex = i;
					map.LocateRoom(x, y);
					break;
				}
			}
		}

		#region Event Handlers

		protected void HandlePreLoad(object sender, EventArgs e)
		{
			this.FindChild<DropDown>("dropDownSelectedMap").SelectedIndex = 0;
			UpdateTitle();
		}

		protected void HandleLoad(object sender, EventArgs e)
		{
			if (File.Exists(".coopconfig"))
			{
				_coOpConfig = new CoOpConfig();
				_coOpConfig.RestoreFrom(".coopconfig");
				CreateCoOpClient();
			}

			if (File.Exists("last.mts"))
			{
				try
				{
					DoLoadSession("last.mts");
				}
				catch
				{
					AssignSessionFlags();
					ResetSessionState();
				}
			}
			else
			{
				AssignSessionFlags();
				ResetSessionState();
			}
		}

		protected void HandleClosing(object sender, EventArgs e)
		{
			try
			{
				DoSaveSession("last.mts");
			}
			catch
			{
				//
			}
		}

		protected void HandleClosed(object sender, EventArgs e)
		{
			foreach (var window in Application.Instance.Windows)
			{
				window.Close();
			}
		}

		#region Menus

		protected void HandleSessionNewClick(object sender, EventArgs e)
		{
			var newSessionFlags = new SessionFlags();
			EditSessionFlagsDlg dlg = new EditSessionFlagsDlg(true);
			dlg.Flags = newSessionFlags;
			dlg.ShowModal(this);
			if (dlg.Result == true)
			{
				_sessionFlags = newSessionFlags;
				AssignSessionFlags();
				ResetSessionState();
				_sessionFilename = null;
				UpdateTitle();
			}
		}

		protected void HandleSessionOpenClick(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filters.Add(new FileFilter("Metal Tracker Session", ".mts"));
			var dr = ofd.ShowDialog(this);
			if (dr == DialogResult.Ok)
			{
				if (LoadSession(ofd.FileName))
				{
					_sessionFilename = ofd.FileName;
					UpdateTitle();
				}
			}
		}

		protected void HandleSessionSaveClick(object sender, EventArgs e)
		{
			if (_sessionFilename == null)
			{
				HandleSessionSaveAsClick(sender, e);
			}
			else
			{
				SaveSession(_sessionFilename);
			}
		}

		protected void HandleSessionSaveAsClick(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filters.Add(new FileFilter("Metal Tracker Session", ".mts"));
			var dr = sfd.ShowDialog(this);
			if (dr == DialogResult.Ok)
			{
				if (SaveSession(sfd.FileName))
				{
					_sessionFilename = sfd.FileName;
					UpdateTitle();
				}
			}
		}

		protected void HandleAppQuitClick(object sender, EventArgs e)
		{
			Application.Instance.Quit();
		}

		protected void HandleShowSessionLogClick(object sender, EventArgs e)
		{
			SessionLogForm form = new SessionLogForm(this);
			form.AddMap(_overworldMap);
			for (int i = 0; i < 9; i++)
			{
				form.AddMap(_dungeonMaps[i]);
			}
			form.AddMap(_zebesMap);
			form.Show();
		}

		protected void HandleSessionEditFlagsClick(object sender, EventArgs e)
		{
			EditSessionFlagsDlg dlg = new EditSessionFlagsDlg(false);
			dlg.Flags = _sessionFlags;
			if (dlg.ShowModal(this))
			{
				AssignSessionFlags();
			}
		}

		protected void HandleSessionClearDataClick(object sender, EventArgs e)
		{
			if (MessageBox.Show("This will reset all tracked data to default for the current flags. Proceed?", "Metal Tracker",
					MessageBoxButtons.YesNo, MessageBoxType.Question) == DialogResult.Yes)
			{
				ResetSessionState();
			}
		}

		protected void HandleOpenCoOpClientClick(object sender, EventArgs e)
		{
			if (_coOpConfig == null)
			{
				MessageBox.Show("We'll need to configure your co-op settings first.", "Metal Tracker", MessageBoxType.Information);
				HandleConfigCoOpClick(sender, e);
			}

			if (_coOpConfig != null)
			{
				if (_coOpClient == null)
				{
					CreateCoOpClient();
				}
				CoOpClientForm _coOpClientForm = new CoOpClientForm(_coOpClient);
				_coOpClientForm.Show(this);
			}
		}

		protected void HandleOpenCoOpRoomClick(object sender, EventArgs e)
		{
			if (_coOpClient == null)
			{
				MessageBox.Show("You must configure and connect the co-op client first.", "Metal Tracker", MessageBoxButtons.OK, MessageBoxType.Error);
				return;
			}

			if (!_coOpClient.IsConnected())
			{
				if (MessageBox.Show("Client is not connected. Open connection window?", "Metal Tracker", MessageBoxButtons.OKCancel,
					MessageBoxType.Question) == DialogResult.Ok)
				{
					HandleOpenCoOpClientClick(sender, e);
				}
			}

			if (_coOpClient.IsConnected())
			{
				JoinRoomDlg dlg = new JoinRoomDlg(_coOpClient);
				dlg.ShowModal(this);
			}
		}

		protected void HandleConfigCoOpClick(object sender, EventArgs e)
		{
			CoOpConfigDlg coOpConfigDlg = new CoOpConfigDlg();
			coOpConfigDlg.Config = _coOpConfig;
			if (coOpConfigDlg.ShowModal(this))
			{
				_coOpConfig = coOpConfigDlg.Config;
				_coOpConfig.PersistTo(".coopconfig");
				if (_coOpClient != null && _coOpClient.IsConnected())
				{
					_coOpClient.UpdatePlayer(_coOpConfig.PlayerName, _coOpConfig.PlayerColor);
				}
			}
		}

		protected void HandleHelpClick(object sender, EventArgs e)
		{
			ProcessStartInfo info = new ProcessStartInfo
			{
				UseShellExecute = true,
				FileName = "readme.txt"
			};

			Process.Start(info);
		}

		protected void HandleAboutClick(object sender, EventArgs e)
		{
			new AboutDlg().ShowModal(this);
		}

		#endregion

		protected void HandleMapChanged(object sender, EventArgs e)
		{
			DropDown dropDown = sender as DropDown;

			_overworldMap.Activate(false);

			for (int i = 0; i < 9; i++)
			{
				_dungeonMaps[i].Activate(false);
			}

			_zebesMap.Activate(false);

			if (dropDown.SelectedIndex == 0)
			{
				_overworldMap.Activate(true);
			}
			else if (dropDown.SelectedIndex == 10)
			{
				_zebesMap.Activate(true);
			}
			else
			{
				int level = dropDown.SelectedIndex;
				_dungeonMaps[level - 1].Activate(true);
			}
		}

		#endregion

		private void UpdateTitle()
		{
			if (_sessionFilename == null)
			{
				this.Title = $"Metal Tracker for Z1M1";
			}
			else
			{
				this.Title = $"Metal Tracker for Z1M1 [{_sessionFilename}]";
			}
		}

		private void CreateCoOpClient()
		{
			_coOpClient = new CoOpClient("https://mtshub.azurewebsites.net");
			_coOpClient.Configure(_coOpConfig.PlayerId, _coOpConfig.PlayerName, _coOpConfig.PlayerColor);
			_overworldMap.SetCoOpClient(_coOpClient);

			for (int i = 0; i < 9; i++)
			{
				_dungeonMaps[i].SetCoOpClient(_coOpClient);
			}

			_zebesMap.SetCoOpClient(_coOpClient);
		}

		private void AssignSessionFlags()
		{
			_overworldMap.SetMapFlags(
				_sessionFlags.ZeldaQ2,
				_sessionFlags.DungeonEntrancesShuffled,
				_sessionFlags.OtherEntrancesShuffled,
				_sessionFlags.OverworldMirrored);

			for (int i = 0; i < 9; i++)
			{
				_dungeonMaps[i].SetMapFlags(_sessionFlags.ZeldaQ2, i + 1, _sessionFlags.DungeonRoomShuffleMode, _sessionFlags.DungeonsMirrored[i]);
			}

			_zebesMap.SetMapFlags(_sessionFlags.ZebesRoomShuffleMode, _sessionFlags.ZebesMirrored);
		}

		private void ResetSessionState()
		{
			_overworldMap.ResetState();

			if (!_sessionFlags.OtherEntrancesShuffled)
			{
				if (_sessionFlags.OverworldMirrored)
				{
					_overworldMap.SetDestination(5, 6, "m1|B");
					_overworldMap.SetDestination(12, 0, "m1|K");
					_overworldMap.SetDestination(11, 1, "m1|N");
					_overworldMap.SetDestination(1, 1, "m1|R");
				}
				else
				{
					_overworldMap.SetDestination(10, 6, "m1|B");
					_overworldMap.SetDestination(3, 0, "m1|K");
					_overworldMap.SetDestination(4, 1, "m1|N");
					_overworldMap.SetDestination(14, 1, "m1|R");
				}
			}

			for (int i = 0; i < 9; i++)
			{
				_dungeonMaps[i].ResetState();
			}

			_zebesMap.ResetState();

			_itemTracker.Init();
		}

		private bool LoadSession(string filename)
		{
			try
			{
				DoLoadSession(filename);
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred while loading:\r\n\r\n{ex.Message}", "Metal Tracker", MessageBoxType.Error);
				return false;
			}
		}

		private void DoLoadSession(string filename)
		{
			string serialized = File.ReadAllText(filename);

			Session session = System.Text.Json.JsonSerializer.Deserialize<Session>(serialized);

			_sessionFlags = session.Flags;

			AssignSessionFlags();

			ResetSessionState();

			_itemTracker.SetInventory(session.Inventory);

			_overworldMap.RestoreState(session.Overworld);
			_dungeonMaps[0].RestoreState(session.Level1);
			_dungeonMaps[1].RestoreState(session.Level2);
			_dungeonMaps[2].RestoreState(session.Level3);
			_dungeonMaps[3].RestoreState(session.Level4);
			_dungeonMaps[4].RestoreState(session.Level5);
			_dungeonMaps[5].RestoreState(session.Level6);
			_dungeonMaps[6].RestoreState(session.Level7);
			_dungeonMaps[7].RestoreState(session.Level8);
			_dungeonMaps[8].RestoreState(session.Level9);
			_zebesMap.RestoreState(session.Zebes);
		}

		private bool SaveSession(string filename)
		{
			try
			{
				DoSaveSession(filename);
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred while saving:\r\n\r\n{ex.Message}", "Metal Tracker", MessageBoxType.Error);
				return false;
			}
		}

		private void DoSaveSession(string filename)
		{
			Session session = new Session
			{
				Flags = _sessionFlags,
				Inventory = _itemTracker.GetInventory(),
				Overworld = _overworldMap.PersistState(),
				Level1 = _dungeonMaps[0].PersistState(),
				Level2 = _dungeonMaps[1].PersistState(),
				Level3 = _dungeonMaps[2].PersistState(),
				Level4 = _dungeonMaps[3].PersistState(),
				Level5 = _dungeonMaps[4].PersistState(),
				Level6 = _dungeonMaps[5].PersistState(),
				Level7 = _dungeonMaps[6].PersistState(),
				Level8 = _dungeonMaps[7].PersistState(),
				Level9 = _dungeonMaps[8].PersistState(),
				Zebes = _zebesMap.PersistState()
			};

			string serialized = System.Text.Json.JsonSerializer.Serialize(session);

			File.WriteAllText(filename, serialized);
		}
	}
}
