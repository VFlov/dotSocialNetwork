using Microsoft.EntityFrameworkCore;
using dotSocialNetwork.Server.Data;
using dotSocialNetwork.Server.Models;
using dotSocialNetwork.Server.Services;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using dotSocialNetwork.Server.Hubs;
public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly List<WebSocket> _sockets = new();

    public ChatService(AppDbContext context, IAuthService authService, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _authService = authService;
        _hubContext = hubContext;
    }

    public async Task<List<Dialog>> GetDialogsAsync()
    {
        var userId = _authService.GetCurrentUserId();
        return await _context.Dialogs
            .Where(d => d.User1Id == userId || d.User2Id == userId)
            .Include(d => d.User1) // Подгружаем User1
            .Include(d => d.User2) // Подгружаем User2
            .Select(d => new Dialog
            {
                Id = d.Id,
                User1Id = d.User1Id,
                User2Id = d.User2Id,
                LastMessage = d.LastMessage,
                User1 = d.User1,
                User2 = d.User2,
                User1UnreadCount = d.User1UnreadCount,
                User2UnreadCount = d.User2UnreadCount
            })
            .ToListAsync();
    }

    public async Task<Dialog> CreateDialogAsync(int targetUserId)
    {
        var currentUserId = _authService.GetCurrentUserId();
        if (currentUserId == targetUserId) throw new Exception("Нельзя создать диалог с самим собой");

        var existingDialog = await _context.Dialogs
            .Include(d => d.User1) // Подгружаем User1
            .Include(d => d.User2) // Подгружаем User2
            .FirstOrDefaultAsync(d =>
                (d.User1Id == currentUserId && d.User2Id == targetUserId) ||
                (d.User1Id == targetUserId && d.User2Id == currentUserId));

        if (existingDialog != null) return existingDialog;

        var dialog = new Dialog
        {
            User1Id = currentUserId,
            User2Id = targetUserId
        };

        _context.Dialogs.Add(dialog);
        await _context.SaveChangesAsync();

        // Загружаем данные пользователей после сохранения
        return await _context.Dialogs
            .Include(d => d.User1)
            .Include(d => d.User2)
            .FirstAsync(d => d.Id == dialog.Id);
    }

    public async Task<List<Message>> GetMessagesAsync(int dialogId)
    {
        var userId = _authService.GetCurrentUserId();
        var dialog = await _context.Dialogs.FindAsync(dialogId);

        if (dialog == null || (dialog.User1Id != userId && dialog.User2Id != userId))
            throw new Exception("Доступ запрещен");

        var messages = await _context.Messages
            .Where(m => m.DialogId == dialogId)
            .OrderBy(m => m.Time)
            .Include(m => m.Sender)
            .ToListAsync();

        // Сбрасываем счетчик непрочитанных для текущего пользователя
        if (dialog.User1Id == userId) dialog.User1UnreadCount = 0;
        else dialog.User2UnreadCount = 0;
        await _context.SaveChangesAsync();

        return messages;
    }

    public async Task<Message> SendMessageAsync(int dialogId, string text, IFormFile? attachment)
    {
        var userId = _authService.GetCurrentUserId();
        var dialog = await _context.Dialogs.FindAsync(dialogId);

        if (dialog == null || (dialog.User1Id != userId && dialog.User2Id != userId))
            throw new Exception("Доступ запрещен");

        var message = new Message
        {
            DialogId = dialogId,
            SenderId = userId,
            Text = text,
            Time = DateTime.Now,
            AttachmentUrl = attachment != null ? await SaveAttachment(attachment) : null
        };

        _context.Messages.Add(message);
        dialog.LastMessage = text;

        if (dialog.User1Id == userId) dialog.User2UnreadCount++;
        else dialog.User1UnreadCount++;

        await _context.SaveChangesAsync();
        await NotifyClients(message);
        return message;
    }

    public async Task<List<Message>> SearchMessagesAsync(string query)
    {
        return await _context.Messages
            .Where(m => m.Text.Contains(query))
            .OrderBy(m => m.Time)
            .Take(50)
            .ToListAsync();
    }



    public async Task NotifyClients(Message message)
    {
        var dialog = await _context.Dialogs.FindAsync(message.DialogId);
        if (dialog != null)
        {
            await _hubContext.Clients.Group(dialog.User1Id.ToString()).SendAsync("ReceiveMessage", message);
            await _hubContext.Clients.Group(dialog.User2Id.ToString()).SendAsync("ReceiveMessage", message);
        }
    }
    private async Task<string> SaveAttachment(IFormFile file)
    {
        var path = Path.Combine("wwwroot/uploads", Guid.NewGuid() + Path.GetExtension(file.FileName));
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{Path.GetFileName(path)}";
    }
}