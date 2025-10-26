using Empath_AI.Data;
using Empath_AI.DTO;
using Empath_AI.Model;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;

namespace Empath_AI.Repository
{
    public class ConversationRepository: IConversationRepository
    {
        private readonly AppDbContext _context;
        public ConversationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Conversation>> GetAll()
        {
            var c = await _context.Conversations.ToListAsync();
            return c;
        }

        public async Task<Conversation?>GetConversationById(int Id)
        {
            return await _context.Conversations.Include(x=>x.user)
                                            //.Include(x=>x.bot).Include(x=x.medical_Report)
                                              .FirstOrDefaultAsync(x => x.Conversations_ID == Id);
        }

        public async Task<Conversation> CreateConversation(ConversationDto conversationDto)
        {
            var conversation = new Conversation()
            {
                 User_ID=conversationDto.User_ID,
                 Title=conversationDto.Title,
                 Created_At=DateTime.Now,
                 Last_Activity=DateTime.Now

            };
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<bool>UpdateTitle(int Id, string NewTitle)
        {
            var conversation = await GetConversationById(Id);
            if(conversation==null)
            {
                return false;
            }
            conversation.Title = NewTitle;
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLastActivity(int Id)
        {
            var con = await GetConversationById(Id);
            if(con==null)
            {
                return false;
            }
            con.Last_Activity = DateTime.Now;
            _context.Conversations.Update(con);
            await _context.SaveChangesAsync();
            return true;      
        }

          public async Task<bool>DeleteConversation( int Id)
        {
            var c = await GetConversationById(Id);
            if (c==null)
            {
                return false;
            }

            _context.Conversations.Remove(c);
            await _context.SaveChangesAsync();
            return true;
        }           





    }
}
