using messageApp_backend.models;
using messageApp_backend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using System;
using System.Reflection.Metadata;
using static messageApp_backend.Models.MessageStatusAttribute;

namespace SignalRProject.Hubs
{
    public class MessageHub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public MessageHub(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task ReceiveMessage(int userId)
        {
            List<string> ContactsConnectionIds = new List<string>();

            using (var scope = _scopeFactory.CreateScope())
            {
                var messageContext = scope.ServiceProvider.GetRequiredService<MessageContext>();
                var teste = await messageContext.Messages
                                         .ToListAsync();
                var messages = await messageContext.Messages
                    .Where(m => m.UsuarioDestinatarioId == userId && m.Status == MessageStatusAttribute.MessageStatus.Sended)
                    .ToListAsync();

                messages.ForEach(m =>
                {
                    var contactConnection = ConnectedUsers.myConnectedUsers.Find(c => c.UserId == m.UsuarioRemetenteId);
                    if (contactConnection != null)
                    {
                        ContactsConnectionIds.Add(contactConnection.ConnectionId);
                        m.Status = MessageStatusAttribute.MessageStatus.Received;
                    }
                });

                await messageContext.SaveChangesAsync();
            }

            foreach (var contactConnectionId in ContactsConnectionIds)
            {
                await Clients.Client(contactConnectionId).SendAsync("MessageReceived", userId);
            }
        }

        public async Task PublishUserOnConnectedAsync(int userId)
        {
            var connectionId = Context.ConnectionId;
            var userConnected = ConnectedUsers.myConnectedUsers.Any(c => c.UserId == userId);

            if (!userConnected)
            {
                ConnectedUser connected_user = new ConnectedUser();
                connected_user.UserId = userId;
                connected_user.ConnectionId = connectionId;
                ConnectedUsers.myConnectedUsers.Add(connected_user);
            }
            else
            {
                var user = ConnectedUsers.myConnectedUsers.Single(c => c.UserId == userId);
                user.ConnectionId = connectionId;
            }

            await ReceiveMessage(userId);

            await Clients.Client(connectionId).SendAsync("ConnectionId", connectionId);
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            ConnectedUser user_to_remove = ConnectedUsers.myConnectedUsers.FirstOrDefault(u => (string?)u.ConnectionId == Context.ConnectionId);
            if (user_to_remove == null) { return base.OnDisconnectedAsync(exception); }
            ConnectedUsers.myConnectedUsers.Remove(user_to_remove);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(Message messageObj, string? imageBase64)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var usersContext = scope.ServiceProvider.GetRequiredService<UserContext>();
                var messagesContext = scope.ServiceProvider.GetRequiredService<MessageContext>();

                var mainUser = usersContext.Users.Find(messageObj.UsuarioRemetenteId);
                var contactUser = usersContext.Users.Find(messageObj.UsuarioDestinatarioId);

                if (mainUser == null || contactUser == null)
                {
                    throw new Exception("Usuário não encontrado");
                }

                if (!contactUser.Contacts.Contains(mainUser))
                {
                    contactUser.Contacts.Add(mainUser);
                }

                if (imageBase64 != null)
                {
                    messageObj.Image = Convert.FromBase64String(imageBase64);
                }
                messageObj.Id = (messagesContext.Messages.Max(m => (int?)m.Id) ?? 0) + 1;
                messagesContext.Messages.Add(messageObj);
                await messagesContext.SaveChangesAsync();
                await usersContext.SaveChangesAsync();
            }
            if (!ConnectedUsers.myConnectedUsers.Any(u => u.UserId == messageObj.UsuarioDestinatarioId))
            {
                return;
            }
            string connection_id = ConnectedUsers.myConnectedUsers.Single(u => u.UserId == messageObj.UsuarioDestinatarioId).ConnectionId;
            await Clients.Client(connection_id).SendAsync("ReceiveMessage", messageObj);
        }


        public async Task EditMessage(Message messageObj)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var messageContext = scope.ServiceProvider.GetRequiredService<MessageContext>();

                Message message = messageContext.Messages.FirstOrDefault(m => m.Id == messageObj.Id);

                message.Conteudo = messageObj.Conteudo;
                messageContext.Update(message);

                await messageContext.SaveChangesAsync();
            }
            if (!ConnectedUsers.myConnectedUsers.Any(u => u.UserId == messageObj.UsuarioDestinatarioId))
            {
                return;
            }
            string connection_id = ConnectedUsers.myConnectedUsers.Single(u => u.UserId == messageObj.UsuarioDestinatarioId).ConnectionId;
            await Clients.Client(connection_id).SendAsync("MessageEdit", messageObj);
        }

        public async Task DeleteMessage(int messageId)
        {
            int usuarioDestinatario;
            using (var scope = _scopeFactory.CreateScope())
            {
                var messageContext = scope.ServiceProvider.GetRequiredService<MessageContext>();
                Message message = messageContext.Messages.Find(messageId);
                messageContext.Remove(message);
                usuarioDestinatario = message.UsuarioDestinatarioId;

                await messageContext.SaveChangesAsync();
            }
            if (!ConnectedUsers.myConnectedUsers.Any(u => u.UserId == usuarioDestinatario))
            {
                return;
            }
            string connection_id = ConnectedUsers.myConnectedUsers.Single(u => u.UserId == usuarioDestinatario).ConnectionId;
            await Clients.Client(connection_id).SendAsync("MessageDelete", usuarioDestinatario);
        }

        public class VisualizeMessageDTO
        {
            public int ContactId { get; set; }
            public int UserId { get; set; }
        }

        public async Task VisualizeMessage(VisualizeMessageDTO visualizeDto)
        {
            bool hasMessages;
            using (var scope = _scopeFactory.CreateScope())
            {
                var messageContext = scope.ServiceProvider.GetRequiredService<MessageContext>();
                var messages = messageContext.Messages
                    .Where(m => m.UsuarioRemetenteId == visualizeDto.ContactId &&
                                m.UsuarioDestinatarioId == visualizeDto.UserId &&
                                m.Status != MessageStatusAttribute.MessageStatus.Visualized)
                    .ToList();
                hasMessages = messages.Count() > 0;

                messages.ForEach(m =>
                {
                    m.Status = MessageStatusAttribute.MessageStatus.Visualized;
                });

                await messageContext.SaveChangesAsync();
            }
            var contactConnection = ConnectedUsers.myConnectedUsers.FirstOrDefault(c => c.UserId == visualizeDto.ContactId);

            if (contactConnection != null && hasMessages)
            {
                await Clients.Client(contactConnection.ConnectionId).SendAsync("MessageVisualized", visualizeDto.UserId);
            }
        }
    }
}
