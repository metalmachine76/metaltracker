using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Eto.Forms;
using Eto.Serialization.Xaml;
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
	internal class MainForm : Form
	{
		private readonly OverworldMap _overworldMap = null;
		private readonly DungeonMap[] _dungeonMaps = new DungeonMap[9];
		private readonly ZebesMap _zebesMap = null;
		private readonly ItemTracker _itemTracker = null;

		private SessionFlags _sessionFlags = new SessionFlags();
		private string _sessionFilename = "default.mts";

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

			for (int i = 0; i < 9; i++)
			{
				var dungeonMap = new DungeonMap(drawableCurrentMap, roomDetailContainer);
				dungeonMap.AddDestinations(zeldaExitDests);
				dungeonMap.AddDestinations(zebesExitDests);
				dungeonMap.SetGameItems(gameItems);
				_dungeonMaps[i] = dungeonMap;
			}

			_zebesMap = new ZebesMap(drawableCurrentMap, roomDetailContainer);
			_zebesMap.AddDestinations(zeldaExitDests);
			_zebesMap.AddDestinations(zebesExitDests);
			_zebesMap.SetGameItems(gameItems);

			var itemTrackerContainer = this.FindChild<GroupBox>("groupBoxItemTracker");
			_itemTracker = new ItemTracker(itemTrackerContainer);
		}

		#region Event Handlers

		protected void HandlePreLoad(object sender, EventArgs e)
		{
			this.FindChild<DropDown>("dropDownSelectedMap").SelectedIndex = 0;
			this.Title = $"Metal Tracker for Z1M1 [{_sessionFilename}]";
			_itemTracker.Init();
		}

		protected void HandleLoad(object sender, EventArgs e)
		{
			if (File.Exists(".coopconfig"))
			{
				_coOpConfig = new CoOpConfig();
				_coOpConfig.RestoreFrom(".coopconfig");
				CreateCoOpClient();
			}

			AssignSessionFlags();
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
			}
		}

		protected void HandleSessionOpenClick(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filters.Add(new FileFilter("Metal Tracker Session", ".mts"));
			var dr = ofd.ShowDialog(this);
			if (dr == DialogResult.Ok)
			{
				// TODO
			}
		}

		protected void HandleSessionSaveClick(object sender, EventArgs e)
		{
			if (_sessionFilename == null)
			{
				SaveSessionAs();
			}
			else
			{
				SaveSession();
			}
		}

		protected void HandleSessionSaveAsClick(object sender, EventArgs e)
		{
			SaveSessionAs();
		}

		protected void HandleAppQuitClick(object sender, EventArgs e)
		{
			Application.Instance.Quit();
		}

		protected void HandleShowSessionLogClick(object sender, EventArgs e)
		{
			SessionLogDlg dlg = new SessionLogDlg();
			dlg.AddMap(_overworldMap);
			for (int i = 0; i < 9; i++)
			{
				dlg.AddMap(_dungeonMaps[i]);
			}
			dlg.AddMap(_zebesMap);
			dlg.ShowModal(this);
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
				_overworldMap.ResetState();
				for (int i = 0; i < 9; i++)
				{
					_dungeonMaps[i].ResetState();
				}
				_zebesMap.ResetState();
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

		protected void HandleHelpAboutClick(object sender, EventArgs e)
		{
			new AboutDialog().ShowDialog(this);
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
				_dungeonMaps[level - 1].SetMapFlags(_sessionFlags.ZeldaQ2, _sessionFlags.DungeonsMirrored[level - 1], level);
				_dungeonMaps[level - 1].Activate(true);
			}
		}

		#endregion

		private void AssignSessionFlags()
		{
			_overworldMap.SetMapFlags(_sessionFlags.ZeldaQ2, _sessionFlags.OverworldMirrored);

			for (int i = 0; i < 9; i++)
			{
				_dungeonMaps[i].SetMapFlags(_sessionFlags.ZeldaQ2, _sessionFlags.DungeonsMirrored[i], i + 1);
			}

			_zebesMap.SetMapFlags(_sessionFlags.ZebesMirrored);
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

		private void SaveSessionAs()
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filters.Add(new FileFilter("Metal Tracker Session", ".mts"));
			var dr = sfd.ShowDialog(this);
			if (dr == DialogResult.Ok)
			{
				_sessionFilename = sfd.FileName;
				SaveSession();
			}
		}

		private void SaveSession()
		{
			// TODO
		}
	}
}
