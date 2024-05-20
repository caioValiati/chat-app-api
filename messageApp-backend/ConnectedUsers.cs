namespace SignalRProject
{
    public class ConnectedUser
    {
        public int UserId { get; set; }
        public string ConnectionId { get; set; }
    }
    public class ConnectedUsers
    {
        public static List<ConnectedUser> myConnectedUsers = new List<ConnectedUser>();
    }
}
