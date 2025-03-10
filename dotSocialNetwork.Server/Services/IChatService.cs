namespace dotSocialNetwork.Server.Services;
using dotSocialNetwork.Server.Models;
using System.Net.WebSockets;

public interface IChatService
{
    Task<List<Dialog>> GetDialogsAsync();
    Task<List<Message>> GetMessagesAsync(int dialogId);
    Task<Message> SendMessageAsync(int dialogId, string text, IFormFile? attachment);
    Task<List<Message>> SearchMessagesAsync(string query);
    Task<Dialog> CreateDialogAsync(int targetUserId);
    Task NotifyClients(Message message);

}