using Empath_AI.Data;
using Empath_AI.DTO.Conversation;
using Empath_AI.DTO.Device;
using Empath_AI.Model;
using Empath_AI.Repository;
using Empath_AI.Service;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Empath_AI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConversationRepository _conversationRepository;
        private readonly Bot _bot;
        private readonly IHeartRateRepository _heart;
        private readonly IMessageRepository _messageService;
        private readonly IHubContext<Hubs.ChatHub> _hubContext;
        public ConversationController(AppDbContext context, IConversationRepository conversationRepository, Bot bot, IHeartRateRepository heart, IMessageRepository messageService, IHubContext<Hubs.ChatHub> hubContext)
        {
            _context = context;
            _conversationRepository = conversationRepository;
            _bot = bot;
            _heart = heart;
            _messageService = messageService;
            _hubContext = hubContext;
        }

        [HttpGet("get-all")] 
         public async Task<IActionResult>GetAll()
        {
            var conversations = await _conversationRepository.GetAll();
            return Ok(conversations);
        }

        [HttpGet ("{id}")]
          public async Task<IActionResult>GetById(int id)
        {
            var con = await _conversationRepository.GetConversationById(id);
            if (con == null)
                return NotFound("Conversation not found");
            return Ok(con);
            
        }

        [HttpGet("User/{UserID}")]
        public async Task<IActionResult> GetByUser(int UserID)
        {
            var conversation = await _conversationRepository.GetConversationByUserId(UserID);
            return Ok(conversation);
        }

        [HttpPost("create")]
        public async Task<IActionResult>Create([FromBody]ConversationDTO conversationDTO)
        {
            /* var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

             if (string.IsNullOrEmpty(userIdClaim))
                 return Unauthorized("User ID not found in token");*/
            int userIdClaim = 7;

            conversationDTO.userid = userIdClaim;
            

            await _conversationRepository.CreateConversation(conversationDTO);
            return Ok("conversation created succsessfully");

        }

        [HttpPost("OpenConversation/{conversationid}")]
        public async Task<IActionResult> OpenConversation(int conversationid)
        {
            var c = await _conversationRepository.OpenConversation (conversationid);
            if (c == null)
                return NotFound("conversation not found");

            return Ok(c);

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









    }
}
