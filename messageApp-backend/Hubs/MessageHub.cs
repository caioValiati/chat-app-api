using messageApp_backend.Models;
using Microsoft.AspNetCore.SignalR;

namespace SignalRProject.Hubs
{
    public class MessageHub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public MessageHub(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task PublishUserOnConnectedAsync(int userId)
        {
            var connectionId = Context.ConnectionId;
            ConnectedUser connected_user = new ConnectedUser();
            connected_user.UserId = userId;
            connected_user.ConnectionId = connectionId;
            ConnectedUsers.myConnectedUsers.Add(connected_user);
            await Clients.Client(connectionId).SendAsync("ConnectionId", connectionId);
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            ConnectedUser user_to_remove = ConnectedUsers.myConnectedUsers.Single(u => u.ConnectionId == Context.ConnectionId);
            ConnectedUsers.myConnectedUsers.Remove(user_to_remove);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(Message messageObj)
        {
            string connection_id = ConnectedUsers.myConnectedUsers.Single(u => u.UserId == messageObj.usuarioDestinatario).ConnectionId;
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MessageContext>();
                context.Messages.Add(messageObj);
                await context.SaveChangesAsync();
            }
            await Clients.Client(connection_id).SendAsync("ReceiveMessage", messageObj);
        }
    }
}
