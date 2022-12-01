using System;
using Eto.Forms;
using Eto.Serialization.Xaml;
using MetalTracker.CoOp;
using MetalTracker.Trackers.Z1M1.Dialogs;
using Microsoft.AspNetCore.SignalR.Client;

namespace MetalTracker.Trackers.Z1M1.Forms
{
	internal class CoOpClientForm : Form
	{
		private CoOpClient _coOpClient;
		private UITimer _uITimer;

		public CoOpClientForm(CoOpClient coOpClient)
		{
			_coOpClient = coOpClient;
			XamlReader.Load(this);
		}

		public void Show(MainForm mainform)
		{
			this.Owner = mainform;
			this.Show();
		}

		protected void HandleLoad(object sender, EventArgs e)
		{
			_uITimer = new UITimer();
			_uITimer.Interval = 0.5;
			_uITimer.Elapsed += HandleTimerElapsed;
			_uITimer.Start();
		}

		protected void HandleLoadComplete(object sender, EventArgs e)
		{
			var loc = this.Location;
			loc.Y = -1000;
			this.Location = loc;
		}

		protected void HandleShown(object sender, EventArgs e)
		{
			var loc = this.Location;
			loc.X = this.Owner.Location.X + this.Owner.Width / 2 - this.Width / 2;
			loc.Y = this.Owner.Location.Y + 100;
			this.Location = loc;
		}

		private void HandleTimerElapsed(object sender, EventArgs e)
		{
			Label labelConnected = this.FindChild<Label>("labelConnected");
			Button buttonConnect = this.FindChild<Button>("buttonConnect");
			Button buttonDisconnect = this.FindChild<Button>("buttonDisconnect");
			Label labelRoomJoined = this.FindChild<Label>("labelRoomJoined");
			LinkButton linkViewRooms = this.FindChild<LinkButton>("linkViewRooms");

			if (_coOpClient.IsErrored())
			{
				labelConnected.Text = "There was an error connecting.";
				buttonConnect.Enabled = true;
				buttonDisconnect.Enabled = false;
			}
			else
			{
				var connState = _coOpClient.GetConnectionState() ?? HubConnectionState.Disconnected;
				if (connState == HubConnectionState.Disconnected)
				{
					labelConnected.Text = "Client is not connected.";
					buttonConnect.Enabled = true;
					buttonDisconnect.Enabled = false;

					labelRoomJoined.Visible = true;
					labelRoomJoined.Text = $"...";
					linkViewRooms.Visible = false;
				}
				else if (connState == HubConnectionState.Connected)
				{
					labelConnected.Text = "Client is connected!";
					buttonConnect.Enabled = false;
					buttonDisconnect.Enabled = true;
					if (_coOpClient.RoomId != null)
					{
						labelRoomJoined.Visible = true;
						labelRoomJoined.Text = $"Room {_coOpClient.RoomId} joined.";
						linkViewRooms.Visible = false;
					}
					else
					{
						labelRoomJoined.Visible = false;
						linkViewRooms.Visible = true;
						linkViewRooms.Text = $"View rooms";
					}
				}
				else if (connState == HubConnectionState.Connecting)
				{
					labelConnected.Text = "Client is connecting...";
					buttonConnect.Enabled = false;
					buttonDisconnect.Enabled = false;
					labelRoomJoined.Visible = true;
					labelRoomJoined.Text = $"...";
					linkViewRooms.Visible = false;
				}
				else
				{
					labelConnected.Text = "Client is re-connecting...";
					buttonConnect.Enabled = false;
					buttonDisconnect.Enabled = false;
					labelRoomJoined.Visible = true;
					labelRoomJoined.Text = $"...";
					linkViewRooms.Visible = false;
				}
			}
		}

		protected void HandleConnectClient(object sender, EventArgs e)
		{
			_coOpClient.Connect();
		}

		protected void HandleDisconnectClient(object sender, EventArgs e)
		{
			_coOpClient.Disconnect();
		}

		protected void HandleViewRoomsClick(object sender, EventArgs e)
		{
			if (_coOpClient.IsConnected())
			{
				JoinRoomDlg dlg = new JoinRoomDlg(_coOpClient);
				dlg.ShowModal(this);
			}
		}
	}
}
