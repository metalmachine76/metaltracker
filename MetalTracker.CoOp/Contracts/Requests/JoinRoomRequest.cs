namespace MetalTrackerServer.CoOp.Contracts.Requests
{
	public class JoinRoomRequest
	{
		public string RoomId { get; set; }

		public string PlayerId { get; set; }

		public string ConnectionId { get; set; }
	}
}
