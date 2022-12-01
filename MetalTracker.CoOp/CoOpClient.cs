using MetalTracker.CoOp.Contracts.Responses;
using MetalTracker.CoOp.EventArgs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace MetalTracker.CoOp
{
	public class CoOpClient : ICoOpClient
	{
		public string ServerAddress { get; private set; }
		public string PlayerId { get; private set; }
		public string PlayerName { get; private set; }
		public string PlayerColor { get; private set; }
		public string RoomId { get; private set; }

		public event EventHandler<FoundEventArgs> FoundItem;
		public event EventHandler<FoundEventArgs> FoundDest;

		private HubConnection _hubConnection;
		private Exception _connectionError;

		public CoOpClient(string serverAddress)
		{
			this.ServerAddress = serverAddress;
		}

		public void Configure(string playerId, string playerName, string playerColor)
		{
			this.PlayerId = playerId;
			this.PlayerName = playerName;
			this.PlayerColor = playerColor;
		}

		public async Task Connect()
		{
			_connectionError = null;

			try
			{
				var hubConnectionBuilder = new HubConnectionBuilder();
				_hubConnection = hubConnectionBuilder
					.WithUrl($"{ServerAddress}/coophub")
					.AddMessagePackProtocol()
					.Build();

				_hubConnection.On<string, string, string, int, int, int, string>("EchoItemLocation", HandleEchoItemLocation);
				_hubConnection.On<string, string, string, int, int, int, string>("EchoDestLocation", HandleEchoDestLocation);

				_hubConnection.Closed += HubConnectionClosed;

				await _hubConnection.StartAsync();

				await _hubConnection.InvokeAsync("JoinLobby", this.PlayerId, this.PlayerName, this.PlayerColor);
			}
			catch (Exception ex)
			{
				_connectionError = ex;
			}
		}

		private async Task HubConnectionClosed(Exception? arg)
		{
			this.RoomId = null;
		}

		public async Task Disconnect()
		{
			try
			{
				await _hubConnection.StopAsync();
				this.RoomId = null;
			}
			catch
			{
				// don't care
			}
		}

		public bool IsConnected()
		{
			return _hubConnection?.State == HubConnectionState.Connected;
		}

		public HubConnectionState? GetConnectionState()
		{
			return _hubConnection?.State;
		}

		public bool IsErrored()
		{
			return _connectionError != null;
		}

		#region Invokes

		public async Task UpdatePlayer(string playerName, string playerColor)
		{
			this.PlayerName = playerName;
			this.PlayerColor = playerColor;
			await _hubConnection.InvokeAsync("JoinLobby", this.PlayerId, this.PlayerName, this.PlayerColor);
		}

		public async Task<List<RoomSummary>> ListRooms()
		{
			return await _hubConnection.InvokeAsync<List<RoomSummary>>("ListRooms");
		}

		public async Task<string> OpenRoom()
		{
			string opened = await _hubConnection.InvokeAsync<string>("OpenRoom");
			this.RoomId = opened;
			return opened;
		}

		public async Task<bool> JoinRoom(string roomId)
		{
			bool joined = await _hubConnection.InvokeAsync<bool>("JoinRoom", roomId);
			if (joined)
			{
				this.RoomId = roomId;
			}
			return joined;
		}

		public async Task<PlayerSummary> GetPlayer(string playerId)
		{
			return await _hubConnection.InvokeAsync<PlayerSummary>("GetPlayer", playerId);
		}

		public async Task SendItemLocation(string game, string map, int x, int y, int slot, string code)
		{
			await _hubConnection.InvokeAsync("SendItemLocation", game, map, x, y, slot, code);
		}

		public async Task SendDestLocation(string game, string map, int x, int y, int slot, string code)
		{
			await _hubConnection.InvokeAsync("SendDestLocation", game, map, x, y, slot, code);
		}

		#endregion

		#region Incoming message handlers

		private void HandleEchoDestLocation(string playerId, string game, string map, int x, int y, int slot, string code)
		{
			if (playerId != this.PlayerId)
			{
				FoundEventArgs args = new FoundEventArgs(playerId, game, map, x, y, slot, code);
				FoundDest?.Invoke(this, args);
			}
		}

		private void HandleEchoItemLocation(string playerId, string game, string map, int x, int y, int slot, string code)
		{
			if (playerId != this.PlayerId)
			{
				FoundEventArgs args = new FoundEventArgs(playerId, game, map, x, y, slot, code);
				FoundItem?.Invoke(this, args);
			}
		}

		#endregion
	}
}
