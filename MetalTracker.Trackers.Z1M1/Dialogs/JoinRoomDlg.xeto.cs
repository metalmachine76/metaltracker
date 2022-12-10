using System;
using Eto.Forms;
using Eto.Serialization.Xaml;
using MetalTracker.CoOp;
using MetalTracker.CoOp.Contracts.Responses;

namespace MetalTracker.Trackers.Z1M1.Dialogs
{
	internal class JoinRoomDlg : Dialog<bool>
	{
		private CoOpClient _coOpClient;

		private RoomSummary _selected;

		public JoinRoomDlg(CoOpClient coOpClient)
		{
			XamlReader.Load(this);

			_coOpClient = coOpClient;
		}

		protected void HandleLoad(object sender, EventArgs e)
		{
			GridView gridView = this.FindChild<GridView>("gridViewRoomList");

			gridView.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell { Binding = Binding.Property<RoomSummary, string>(r => r.RoomId) },
				HeaderText = "Room ID",
				Width = 100
			});

			gridView.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell { Binding = Binding.Property<RoomSummary, string>(r => r.OwnerName) },
				HeaderText = "Room Owner",
				Width = 200
			});

			gridView.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell { Binding = Binding.Property<RoomSummary, string>(r => r.PlayerCount.ToString()) },
				HeaderText = "Players",
				Width = 50
			});
		}

		protected void HandleShown(object sender, EventArgs e)
		{
			RefreshList();
		}

		protected void HandleRefresh(object sender, EventArgs e)
		{
			RefreshList();
		}

		protected void HandleSelectionChanged(object sender, EventArgs e)
		{
			GridView gridView = this.FindChild<GridView>("gridViewRoomList");
			_selected = gridView.SelectedItem as RoomSummary;
			this.FindChild<Button>("buttonJoinRoom").Enabled = _selected != null;
		}

		protected async void HandleOpenRoomClick(object sender, EventArgs e)
		{
			string roomId = await _coOpClient.OpenRoom();
			if (roomId != null)
			{
				MessageBox.Show($"Room {roomId} created and joined!", "Metal Tracker", MessageBoxButtons.OK, MessageBoxType.Information);
				this.Close();
			}
			else
			{
				MessageBox.Show($"Could not create new room.", "Metal Tracker", MessageBoxButtons.OK, MessageBoxType.Error);
			}
		}

		protected async void HandleJoinRoomClick(object sender, EventArgs e)
		{
			string roomId = _selected.RoomId;
			bool joined = await _coOpClient.JoinRoom(roomId);
			if (joined)
			{
				MessageBox.Show($"Room {roomId} joined!", "Metal Tracker", MessageBoxButtons.OK, MessageBoxType.Information);
				this.Close();
			}
			else
			{
				MessageBox.Show($"Could not join room.", "Metal Tracker", MessageBoxButtons.OK, MessageBoxType.Error);
			}
		}

		private async void RefreshList()
		{
			GridView gridView = this.FindChild<GridView>("gridViewRoomList");
			gridView.DataStore = null;
			var rooms = await _coOpClient.ListRooms();
			gridView.DataStore = rooms;
		}
	}
}
