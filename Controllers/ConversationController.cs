using Empath_AI.Data;
using Empath_AI.DTO.Conversation;
using Empath_AI.Model;
using Empath_AI.Repository;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Empath_AI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConversationRepository _conversationRepository;
        public ConversationController(AppDbContext context,IConversationRepository conversationRepository)
        {
            _context = context;
            _conversationRepository = conversationRepository;
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
                return (NotFound("Conversation not found"));
            return Ok(con);
            
        }

        [HttpGet("User/{UserId}")]
        public async Task<IActionResult> GetByUser(int UserID)
        {
            var conversation = await _conversationRepository.GetConversationByUserId(UserID);
            return Ok(conversation);
        }

        [HttpPost("{UserId}")]
        public async Task<IActionResult>Create(int UserId, [FromBody]ConversationDTO conversationDTO)
        {
            var user = await _context.Users.FindAsync(UserId); 
            if (user == null)
                return NotFound("User not found");
            await _conversationRepository.CreateConversation(conversationDTO, user);
            return Ok("conversation created succsessfully");

        }











   

        //    [HttpGet("user/{userId}")] GetByUser(int userId)


        //[HttpPost] Create



















        //post
        //   [HttpPut("{id}/title")] UpdateTitle

        //post
        //    [HttpDelete("{id}")] Delete


        // [HttpGet("search")] Search(*/

        // [HttpPost("search")] Search(*/


        /*[HttpGet("recent")] GetRecent(*/


        /*    [HttpGet("summaries/{userId}")] GetSummaries(int*/


        //   [HttpPut("{id}/archive")] Archive    =>   upadte




    }
}
