using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotSocialNetwork.Server.Models;
using dotSocialNetwork.Server.Services;

namespace dotSocialNetwork.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IAuthService _authService;

        public ChatController(IChatService chatService, IAuthService authService)
        {
            _chatService = chatService;
            _authService = authService;
        }

        [HttpGet("dialogs")]
        public async Task<ActionResult<List<Dialog>>> GetDialogs()
        {
            return Ok(await _chatService.GetDialogsAsync());
        }

        [HttpPost("dialogs")]
        public async Task<ActionResult<Dialog>> CreateDialog([FromBody] int targetUserId)
        {
            var dialog = await _chatService.CreateDialogAsync(targetUserId);
            return CreatedAtAction(nameof(GetDialogs), new { id = dialog.Id }, dialog);
        }

        [HttpGet("messages/{dialogId}")]
        public async Task<ActionResult<List<Message>>> GetMessages(int dialogId)
        {
            return Ok(await _chatService.GetMessagesAsync(dialogId));
        }

        [HttpPost("messages/{dialogId}")]
        public async Task<ActionResult<Message>> SendMessage(int dialogId, [FromForm] string text, IFormFile? attachment)
        {
            try
            {
                var message = await _chatService.SendMessageAsync(dialogId, text, attachment);
                return CreatedAtAction(nameof(GetMessages), new { dialogId = message.DialogId }, message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("search-users")]
        public async Task<ActionResult<List<User>>> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { Message = "Запрос поиска не может быть пустым" });

            var users = await _authService.SearchUsersAsync(query);
            return Ok(users);
        }
    }
}
