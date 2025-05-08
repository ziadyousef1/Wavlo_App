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
        private readonly ChatDbContext _chatDb;
        private readonly IChatRepository _repository;
        private readonly IHubContext<ChatHub> _context;

        public ChatController(IChatRepository repository , IHubContext<ChatHub> context , ChatDbContext chatDb)
        {
            _context = context;
            _repository = repository;
            _chatDb = chatDb;
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _chatDb.Users.ToListAsync();
            return Ok(users);
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

            
            var chatDtos = chats.Select(c => new ChatDto
            {
                Id = c.Id,
                Name = c.Name,
                IsGroup = c.IsGroup,
                Type = c.Type.ToString(),
                UserIds = c.Users.Select(u => u.UserId).ToList()
            }).ToList();

            return Ok(chatDtos);
        }

        [HttpGet("private")]
        public async Task<IActionResult> PrivateChat()
        {
            var chats = await _repository.GetPrivateChatsAsync(GetUserId());

            var chatDtos = chats.Select(c => new ChatDto
            {
                Id = c.Id,
                Name = c.Name,
                IsGroup = c.IsGroup,
                Type = c.Type.ToString(),
                UserIds = c.Users.Select(u => u.UserId).ToList()
            }).ToList();

            return Ok(chatDtos);
        }
        [HttpPost("create-room")]
        public async Task<IActionResult> CreateRoom(string name)
        {
            //if (!int.TryParse(User.GetUserId(), out int rootUserId))
            //{
            //    return BadRequest("Invalid user ID");
            //}
            var userId = GetUserId();  
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var chatId = await _repository.CreateRoomAsync(name, userId);
            return Ok(new { chatId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Chat(int id)
        {
            var chat = await _repository.GetChatAsync(id);
            return Ok(chat);
        }
        [HttpPost("create-private-chat")]

        public async Task<IActionResult> CreatePrivateChat(string userId , string name)
        {
            var rootUserId = User.GetUserId();
            if (string.IsNullOrEmpty(rootUserId))
            {
                return BadRequest("Invalid user ID");
            }

            var chatId = await _repository.CreatePrivateRoomAsync(rootUserId, userId , name);
            return Ok(new { chatId });
        }

        [HttpPost("join-room")]
        public async Task<IActionResult> JoinRoom(int id)
        {
            //if (!int.TryParse(User.GetUserId(), out int userId))
            //{
            //    return BadRequest("Invalid user ID");
            //}

            var joined = await _repository.JoinRoomAsync(id, GetUserId());
            if (!joined)
                return BadRequest("Failed to join room");

            return Ok("Joined successfully");
        }
        [HttpPost("leave-room")]
        public async Task<IActionResult> LeaveRoom(int chatId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found");
            var left = await _repository.LeaveRoomAsync(chatId, userId);
            if (!left)
                return BadRequest("You are not a member of this Group!");

            return Ok("Left successfully");
        }


        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            if (dto.RoomId <= 0 || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Invalid room ID or empty message");

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found");


            var chatExists = await _chatDb.Chats.AnyAsync(c => c.Id == dto.RoomId);
            if (!chatExists)
                return NotFound("Chat room not found");

            string attachmentType = "Text";
            if (!string.IsNullOrEmpty(dto.AttachmentUrl))
            {
                var fileName = Path.GetFileName(dto.AttachmentUrl); 
                var extension = Path.GetExtension(fileName)?.ToLower(); 

                if (new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }.Contains(extension))
                    attachmentType = "Image";
                else if (new[] { ".mp4", ".avi", ".mov", ".mkv" }.Contains(extension))
                    attachmentType = "Video";
                else if (new[] { ".mp3", ".wav", ".ogg", ".m4a" }.Contains(extension))
                    attachmentType = "Audio";
                else
                    attachmentType = "File";
            }

            
            var messageEntity = await _repository.CreateMessageAsync(dto.RoomId, dto.Message, userId, dto.TargetUserId, dto.AttachmentUrl);

            if (messageEntity == null)
                return BadRequest("Failed to send message");

            
            if (dto.IsPrivate && !string.IsNullOrEmpty(dto.TargetUserId))
            {
                var targetUserExists = await _chatDb.Users.FindAsync(dto.TargetUserId);
                if (targetUserExists == null)
                    return NotFound("Target user not found");

                await _context.Clients.User(dto.TargetUserId)
                    .SendAsync("ReceiveMessage", new
                    {
                        Text = messageEntity.Content,
                        AttachmentUrl = messageEntity.AttachmentUrl,
                        AttachmentType = attachmentType,
                        Name = messageEntity.Name,
                        SentAt = messageEntity.SentAt.ToString("dd/MM/yyyy hh:mm:ss")
                    });
            }
            else
            {
                
                await _context.Clients.Group(dto.RoomId.ToString())
                    .SendAsync("ReceiveMessage", new
                    {
                        Text = messageEntity.Content,
                        AttachmentUrl = messageEntity.AttachmentUrl,
                        AttachmentType = attachmentType,
                        Name = messageEntity.Name,
                        SentAt = messageEntity.SentAt.ToString("dd/MM/yyyy hh:mm:ss")
                    });
            }

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
