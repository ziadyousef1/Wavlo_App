using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wavlo.Repository;
using Microsoft.AspNetCore.Authorization;
using Wavlo.Data;
using Microsoft.EntityFrameworkCore;
using Wavlo.DTOs;

namespace Wavlo.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : BaseController
    {
        private readonly IChatRepository _repository;
        private readonly IHubContext<ChatHub> _context;

        public ChatController(IChatRepository repository , IHubContext<ChatHub> context)
        {
            _context = context;
            _repository = repository;
        }
        [HttpGet("find")]
        public async Task<IActionResult> Find([FromServices] ChatDbContext context)
        {
           

            var users = await context.Users
                .Where(x => x.Id != User.GetUserId())
                .ToListAsync();

            return Ok(users);
        }
        [HttpGet("chats")]
        public async Task<IActionResult> GetChats()
        {
            

            var chats = await _repository.GetChatsAsync(GetUserId());
            return Ok(chats);
        }

        [HttpGet("private")]
        public async Task<IActionResult> PrivateChat()
        {
           

            var chats = await _repository.GetPrivateChatsAsync(GetUserId());
            return Ok(chats);
        }
        [HttpPost("create-room")]
        public async Task<IActionResult> CreateRoom(string name, string userId)
        {
            if (!int.TryParse(User.GetUserId(), out int rootUserId))
            {
                return BadRequest("Invalid user ID");
            }

            var chatId = await _repository.CreateRoomAsync(GetUserId(), userId);
            return Ok(new { chatId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Chat(int id)
        {
            var chat = await _repository.GetChatAsync(id);
            return Ok(chat);
        }
        [HttpPost("create-private-chat")]
        public async Task<IActionResult> CreatePrivateChat(string userId)
        {
            if (!int.TryParse(User.GetUserId(), out int rootUserId))
            {
                return BadRequest("Invalid user ID");
            }

            var chatId = await _repository.CreatePrivateRoomAsync(GetUserId(), userId);
            return Ok(new { chatId });
        }

        [HttpPost("join-room")]
        public async Task<IActionResult> JoinRoom(int id)
        {
            if (!int.TryParse(User.GetUserId(), out int userId))
            {
                return BadRequest("Invalid user ID");
            }

            var joined = await _repository.JoinRoomAsync(id, GetUserId());
            if (!joined)
                return BadRequest("Failed to join room");

            return Ok("Joined successfully");
        }



        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            if (dto.RoomId <= 0 || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Invalid room ID or empty message");

            string? attachmentType = null;

            if (!string.IsNullOrEmpty(dto.AttachmentUrl))
            {
                var extension = Path.GetExtension(dto.AttachmentUrl).ToLower();
                if (new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }.Contains(extension))
                    attachmentType = "Image";
                else if (new[] { ".mp4", ".avi", ".mov", ".mkv" }.Contains(extension))
                    attachmentType = "Video";
                else if (new[] { ".mp3", ".wav", ".ogg", ".m4a" }.Contains(extension))
                    attachmentType = "Audio";
                else
                    attachmentType = "File";
            }

            var messageEntity = await _repository.CreateMessageAsync(dto.RoomId, dto.Message, GetUserId() , dto.AttachmentUrl);

            if (messageEntity == null)
                return BadRequest("Failed to send message");

            await _context.Clients.Group(dto.RoomId.ToString())
                .SendAsync("ReceiveMessage", new
                {
                    Text = messageEntity.Content,
                    AttachmentUrl = messageEntity.AttachmentUrl,
                    AttachmentType = attachmentType,
                    Name = messageEntity.Name,
                    SentAt = messageEntity.SentAt.ToString("dd/MM/yyyy hh:mm:ss")
                });

            return Ok(new { messageId = messageEntity.Id, message = "Message sent successfully" });
        }
        [HttpPut("edit-message")]
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessageDto dto)
        {
            if (messageId <= 0)
                return BadRequest("Invalid message ID");

            var result = await _repository.EditMessageAsync(messageId, dto.NewContent, dto.NewAttachmentUrl);
            if (!result)
                return BadRequest("Failed to edit message");

            return Ok("Message updated successfully");
        }

        [HttpDelete("delete-message")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            if (messageId <= 0)
                return BadRequest("Invalid message ID");

            var result = await _repository.DeleteMessageAsync(messageId);
            if (!result)
                return BadRequest("Failed to delete message");

            return Ok("Message deleted successfully");
        }



    }
}
