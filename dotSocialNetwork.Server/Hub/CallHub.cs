using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;

namespace dotSocialNetwork.Server.Hub
{

    public class CallHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> RoomParticipants = new();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Rooms = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await base.OnConnectedAsync();
        }

        public async Task CreateRoom(string roomName, string userId) // Добавьте async Task
        {
            Console.WriteLine($"CreateRoom: roomName={roomName}, userId={userId}");
            Rooms[roomName] = new ConcurrentDictionary<string, string>();
            Rooms[roomName][Context.ConnectionId] = userId;
            RoomParticipants[Context.ConnectionId] = roomName;
            await GetRoomListForAll(); // Уведомляем всех
        }
        private async Task GetRoomListForAll()
        {
            var roomList = Rooms
                .Where(r => !string.IsNullOrEmpty(r.Key)) // Фильтруем пустые имена
                .Select(r => new { Name = r.Key, ParticipantCount = r.Value.Count })
                .ToList();
            Console.WriteLine($"GetRoomListForAll: Sending {roomList.Count} rooms to all clients");
            await Clients.All.SendAsync("ReceiveRoomList", roomList);
        }

        public void RemoveRoom(string roomName)
        {
            Console.WriteLine($"RemoveRoom: roomName={roomName}");
            Rooms.TryRemove(roomName, out _);
        }

        public async Task GetRoomList()
        {
            var roomList = Rooms.Select(r => new { Name = r.Key, ParticipantCount = r.Value.Count }).ToList();
            Console.WriteLine($"GetRoomList: Sending {roomList.Count} rooms");
            await Clients.Caller.SendAsync("ReceiveRoomList", roomList);
        }

        public async Task JoinRoom(string roomName, string userId)
        {
            Console.WriteLine($"JoinRoom: roomName={roomName}, userId={userId}, ConnectionId={Context.ConnectionId}");
            if (string.IsNullOrEmpty(roomName))
            {
                Console.WriteLine("JoinRoom: Room name is empty, using default 'UnnamedRoom'");
                roomName = "UnnamedRoom";
            }
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("JoinRoom: User ID is empty, using ConnectionId as fallback");
                userId = Context.ConnectionId;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            if (!Rooms.ContainsKey(roomName))
            {
                await CreateRoom(roomName, userId); // Используем async версию
            }
            else
            {
                Rooms[roomName][Context.ConnectionId] = userId;
                RoomParticipants[Context.ConnectionId] = roomName;
            }

            await Clients.Group(roomName).SendAsync("UserJoined", Context.ConnectionId, userId);
            await Clients.Caller.SendAsync("JoinedRoom", roomName);
            await GetRoomListForAll(); // Уведомляем всех
        }

        public async Task LeaveRoom()
        {
            if (RoomParticipants.TryGetValue(Context.ConnectionId, out var roomName))
            {
                Console.WriteLine($"LeaveRoom: roomName={roomName}, ConnectionId={Context.ConnectionId}");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);

                if (Rooms.TryGetValue(roomName, out var participants))
                {
                    participants.TryRemove(Context.ConnectionId, out _);
                    if (participants.Count == 0)
                    {
                        RemoveRoom(roomName);
                    }
                }

                RoomParticipants.TryRemove(Context.ConnectionId, out _);
                await Clients.Group(roomName).SendAsync("UserLeft", Context.ConnectionId);
                await Clients.Caller.SendAsync("LeftRoom", roomName);
                await GetRoomListForAll(); // Уведомляем всех
            }
        }

        public async Task SendSignal(string signal, string targetConnectionId)
        {
            Console.WriteLine($"SendSignal: targetConnectionId={targetConnectionId}");
            if (string.IsNullOrEmpty(signal) || string.IsNullOrEmpty(targetConnectionId))
            {
                Console.WriteLine("SendSignal: Signal or targetConnectionId is empty");
                return;
            }
            await Clients.Client(targetConnectionId).SendAsync("ReceiveSignal", Context.ConnectionId, signal);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await LeaveRoom();
            Console.WriteLine($"CallHub: ConnectionId={Context.ConnectionId}, Exception={exception?.Message}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}