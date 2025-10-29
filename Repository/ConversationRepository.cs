using Empath_AI.Data;
using Empath_AI.DTO.Conversation;
using Empath_AI.Model;
using FirebaseAdmin.Messaging;
using Google.Api;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

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

        public async Task<List<Conversation>> GetConversationBtUserId(int UserId)
        {
            return await _context.Conversations
                .Where(x => x.User_ID == UserId)
                .OrderByDescending(c => c.Last_Activity)  
                .ToListAsync();
        }

        public async Task CreateConversation(ConversationDTO conversationDto,User user)
        {
            var conversation = new Conversation()
            {
                 User_ID=user.Id,
                 Title=conversationDto.Title,
                 Created_At=DateTime.UtcNow,
                 Last_Activity=DateTime.UtcNow

            };
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();
            
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
            con.Last_Activity = DateTime.UtcNow;
            _context.Conversations.Update(con);
            await _context.SaveChangesAsync();
            return true;      
        }

          public async Task DeleteConversation(Conversation conversation )
        {
            
            _context.Conversations.Remove(conversation);
            await _context.SaveChangesAsync();
            
        }    
        
         public async Task<IEnumerable<Conversation>>SearchConversationByTitle(int UserId,string KeyWord)
        {
            return await _context.Conversations.Where(x => x.User_ID == UserId && x.Title.Contains(KeyWord))
                                               .OrderByDescending(x => x.Last_Activity)
                                               .ToListAsync(); 

        }

         public async Task<IEnumerable<Conversation>> GetRecentConversations(int UserId,int days=7)
        {
            var since = DateTime.UtcNow.AddDays(-days);
            return await _context.Conversations
               .Where(x => x.User_ID == UserId && x.Last_Activity >= since)
               .OrderByDescending(x => x.Last_Activity)
               .ToListAsync();

        }

        //public async Task<List<Message>> GetConversationMessages(int conversationId)
        //{
        //    return await _context.Messages
        //        .Where(m => m.Conversation_ID == conversationId)
        //        .OrderBy(m => m.Created_At)
        //        .ToListAsync();
        //}

        public async  Task<List<ConversationSummaryDTO>> GetConversationSummeries(int UserID)
        {
            return await _context.Conversations
                                 .Where(c => c.User_ID == UserID)
                                 .OrderByDescending(c => c.Last_Activity)
                                  .Select(c => new ConversationSummaryDTO
                                  {
                                      Conversations_ID = c.Conversations_ID,
                                      Title = c.Title,
                                      Last_Activity = c.Last_Activity,
                                      //lastMessage=c.messages.OrderByDescending(m=>m.Created_At).FirtsOrDefault().Content
                                  })
                                  .ToListAsync();                     
        }

        public async Task<bool> ArchiveConversation(int conversationid)
        {
            var con = await GetConversationById(conversationid);
            if(con==null)
            {
                return false;
            }
            con.Is_Archived = true;
            _context.Conversations.Update(con);
            _context.SaveChangesAsync();
            return true;
        }




    }
}
