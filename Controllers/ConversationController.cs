using Empath_AI.Data;
using Empath_AI.DTO.Conversation;
using Empath_AI.DTO.Device;
using Empath_AI.Migrations;
using Empath_AI.Model;
using Empath_AI.Repository;
using Empath_AI.Service;
using Empath_AI.Services;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Empath_AI.Controllers
{
   // [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConversationRepository _conversationRepository;
        private readonly IGeminiService gemin;
        private readonly IHeartRateRepository _heart;
        private readonly IMessageRepository _messageService;
        private readonly IHubContext<Hubs.ChatHub> _hubContext;
        public ConversationController(AppDbContext context, IConversationRepository conversationRepository, IHeartRateRepository heart, IMessageRepository messageService, IHubContext<Hubs.ChatHub> hubContext, IGeminiService gemin)
        {
            _context = context;
            _conversationRepository = conversationRepository;
            _heart = heart;
            _messageService = messageService;
            _hubContext = hubContext;
            this.gemin = gemin;
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("get-all")] 
         public async Task<IActionResult>GetAll()
        {
            var conversations = await _conversationRepository.GetAll();
            return Ok(conversations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var con = await _conversationRepository.GetConversationById(id);
            if (con == null)
                return NotFound("Conversation not found");
            return Ok(con);

        }


        [Authorize]
        [HttpGet("User")]

        public async Task<IActionResult> GetUserConversations()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User not found in token");

            int userId = int.Parse(userIdClaim.Value);

            var conversations = await _conversationRepository.GetConversationByUserId(userId);

            if (conversations == null || !conversations.Any())
                return NotFound("No conversations found for this user");

            var conversation = await _conversationRepository.GetConversationByUserId(userId);
            
            return Ok(userIdClaim);
        }


        //[HttpPost("create")]
        //public async Task<IActionResult> Create([FromBody] ConversationDTO conversationDTO)
        //{
        //    if (conversationDTO.userid <= 0)
        //        return BadRequest("User ID must be provided");

        //    await _conversationRepository.CreateConversation(conversationDTO);
        //    return Ok("Conversation created successfully");
        //}


        //[Authorize]
        //[HttpPost("create")]
        //public async Task<IActionResult> Create([FromBody] ConversationDTO conversationDTO)
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (string.IsNullOrEmpty(userIdClaim))
        //        return Unauthorized("User ID not found in token");


        //    conversationDTO.userid =  int.Parse( userIdClaim);


        //    await _conversationRepository.CreateConversation(conversationDTO);
        //    return Ok("conversation created succsessfully");



        //}

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");

            var userId = int.Parse(userIdClaim);

            var conversationId = await _conversationRepository.CreateConversation(userId);

            return Ok(new { conversationId });
        }










        //[HttpDelete("delete")]
        //public async Task<IActionResult> Delete()
        //{
        //    var userId = int.Parse(User.FindFirst("UserId").Value);

        //    var conv = await _conversationRepository.GetConversationByUserId(userId);

        //    if (conv == null)
        //        return NotFound("Conversation not found");

        //    await _conversationRepository.DeleteConversation();
        //    return Ok("Conversation deleted");
        //}


        [HttpDelete("id")] 
        public async Task<IActionResult> Delete(int id)
        {
            var conv = await _conversationRepository.GetConversationById(id); 
            if (conv == null) 
                return NotFound("Conversation not found");

            await _conversationRepository.DeleteConversation(conv); 

            return Ok("Conversation deleted"); 
        }



        [HttpPost("OpenConversation/{conversationid}")]
        public async Task<IActionResult> OpenConversation(int conversationid)
        {
            var c = await _conversationRepository.OpenConversation(conversationid);
            if (c == null)
                return NotFound("conversation not found");

            return Ok(c);

        }


        [HttpGet("ConversationHistory/{userId}")]
        public async Task<IActionResult> ConversationHistory(int userId)
        {
            var result = await _conversationRepository.ConversationHistory(userId);
            return Ok(result);
        }


        [HttpGet("ConversationHistoryWithMessages/{userId}")]
        public async Task<IActionResult> GetConversationHistory(int userId)
        {
            var result = await _conversationRepository.GetConversationHistoryWithMessages(userId);

            if (result == null || !result.Any())
                return NotFound(new { message = "No conversations found for this user." });

            return Ok(result);
        }

        [HttpGet("Get_messages")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var messages = await _conversationRepository.GetConversationMessages(conversationId);
            return Ok(messages);
        }


        [HttpPut("archive/{id}")]
        public async Task<IActionResult> Archive(int id)
        {
            var ok = await _conversationRepository.ArchiveConversation(id);
            if (!ok)
                return NotFound("Conversation not found");

            return Ok("Conversation archived");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(int userId, string keyword)
        {
            var result = await _conversationRepository.SearchConversationByTitle(userId, keyword);
            return Ok(result);
        }

        [HttpPut("update-title/{id}")]
        public async Task<IActionResult> UpdateTitle(int id, [FromQuery] string newTitle)
        {
            var updated = await _conversationRepository.UpdateTitle(id, newTitle);
            if (!updated)
                return NotFound("Conversation not found");

            return Ok("Title updated");
        }


        [HttpGet("RecentConversations/{userId}")]  // بنجيب لسته باخر واحدث محادثات خلال اخر سبع ايام
        public async Task<IActionResult> RecentConversations(int userId, [FromQuery] int days = 7)
        {
            var result = await _conversationRepository.GetRecentConversations(userId, days);
            return Ok(result);
        }












        /*[HttpPost("Send-message")]
        public async Task<IActionResult> SendMessage([FromBody] DeviceRegisterDTO model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");

            model.UserId = int.Parse(userIdClaim);

            var result = await _hubContext.SendMessage();

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { message = result.Message });
        }*/



        //[HttpPost("AddMessage/{conversationId}")]
        //public async Task<IActionResult> AddMessage(int conversationId, [FromBody] Message message)
        //{
        //    var conversation = await _conversationRepository.GetConversationById(conversationId);
        //    if (conversation == null)
        //        return NotFound("Conversation not found");

        //    message.Conversation_ID = conversationId;
        //    if (message.User_ID == 0)
        //        message.User_ID = conversation.User_ID;

        //    var savedMessage = await _messageRepository.SaveMessageAsync(message);
        //    return Ok(savedMessage);
        //}



        //[HttpPost("create")]
        //public async Task<IActionResult> Create([FromBody] ConversationDTO conversationDTO)
        //{
        //    /* var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //     if (string.IsNullOrEmpty(userIdClaim))
        //         return Unauthorized("User ID not found in token");*/
        //    int userIdClaim = 7;

        //    conversationDTO.userid = userIdClaim;


        //    await _conversationRepository.CreateConversation(conversationDTO);
        //    return Ok("conversation created succsessfully");

        //}





        //[HttpGet("User's Conversations")]
        //public async Task<IActionResult> GetByUser(int UserID)
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (string.IsNullOrEmpty(userIdClaim))
        //        return Unauthorized("User ID not found in token");

        //    //var conversation = await _conversationRepository.GetConversationByUserId(userIdClaim);
        //    return Ok(userIdClaim);
        //}











    }
}
