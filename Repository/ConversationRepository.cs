using Empath_AI.Data;
using Empath_AI.DTO.Conversation;
using Empath_AI.Migrations;
using Empath_AI.Model;
using FirebaseAdmin.Messaging;
using Google.Api;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<List<Conversation>> GetConversationByUserId(int UserId)
        {
            return await _context.Conversations
                .Where(x => x.User_ID == UserId)
                .OrderByDescending(c => c.Last_Activity)  
                .ToListAsync();
        }

        public async Task CreateConversation(ConversationDTO conversationDto)
        {

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var conversation = new Conversation()
            {
                 User_ID=conversationDto.userid,
                 Title=conversationDto.Title,
                 Created_At=egyptTime,
                 Last_Activity=egyptTime

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
                              //FirebaseAdmin.Messaging.Message
        public async Task<List<Empath_AI.Model.Message>> GetConversationMessages(int conversationId)
        {
            return await _context.Messages
                .Where(m => m.Conversation_ID == conversationId)
                .OrderBy(m => m.Created_At)
                .ToListAsync();
        }


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
                                      //LastMessage = c.messages.OrderByDescending(m=>m.Created_At).FirstOrDefault().Content
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
        




        public async Task<ConversationContentDTO> OpenConversation(int conversationid)
        {
            var conversation = await _context.Conversations
                .Include(c => c.messages)
                .FirstOrDefaultAsync(c => c.Conversations_ID == conversationid);

            if (conversation == null)
                return null;

            return new ConversationContentDTO
            {
                ConversationId = conversation.Conversations_ID,
                Title = conversation.Title,
                Messages = conversation.messages
                    .OrderBy(m => m.Created_At)
                    .Select(m => new MessageDTO
                    {
                        Sender_Type = m.Sender_Type,
                        Text = m.Content,
                        Time = m.Created_At.UtcDateTime
                    }).ToList()
            };
        }


        //public async Task<IEnumerable<ConversationSummaryDTO>> ConversationHistory(int userId)
        //{
        //    var conversations = await _context.Conversations
        //        .Where(c => c.User_ID == userId && !c.Is_Archived)
        //        .OrderByDescending(c => c.Last_Activity)
        //        .ToListAsync();

        //    var summaries = new List<ConversationSummaryDTO>();

        //    foreach (var conv in conversations)
        //    {
        //        summaries.Add(new ConversationSummaryDTO
        //        {
        //            Conversations_ID = conv.Conversations_ID,
        //            Title = !string.IsNullOrWhiteSpace(conv.Title)
        //                    ? conv.Title
        //                    : GenerateTitle(conv.FirstMessage),
        //            Last_Activity = conv.Last_Activity,
        //            LastMessage = null
        //        });
        //    }

        //    return summaries;
        //}


        ////GenerateTitle هي مجرد دالة مساعدة (helper) وظيفتها: تاخد نص أول رسالة وترجع عنوان
        ////مش لازم تكون في الـ interface (IConversationRepository)
        //private string GenerateTitle(string message)
        //{
        //    if (string.IsNullOrWhiteSpace(message))
        //        return "New Conversation";

        //    var words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        //    // لو الرسالة أقل من 5 كلمات رجّعها كلها
        //    if (words.Length <= 5)
        //        return string.Join(" ", words);

        //    // غير كده رجّع أول 5 كلمات
        //    return string.Join(" ", words.Take(5)) + "...";
        //}



        //------------------------------------------------------------------------------------------------------------

        //public async Task<IEnumerable<ConversationContentDTO>> GetConversationHistoryWithMessages(int userId)
        //{
        //    var conversations = await _context.Conversations
        //        .Where(c => c.User_ID == userId && !c.Is_Archived)
        //        .Include(c => c.messages)           // ⬅️ جلب كل الرسائل
        //        .OrderByDescending(c => c.Last_Activity) // ⬅️ ترتيب من الأحدث للأقدم
        //        .ToListAsync();

        //    var result = new List<ConversationContentDTO>();

        //    foreach (var conv in conversations)
        //    {
        //        result.Add(new ConversationContentDTO
        //        {
        //            ConversationId = conv.Conversations_ID,
        //            Title = !string.IsNullOrWhiteSpace(conv.Title)
        //                    ? conv.Title
        //                    : GenerateTitle(conv.FirstMessage), // لو عايز تولد عنوان تلقائي
        //            Messages = conv.messages
        //                        .OrderBy(m => m.Created_At) // ترتيب الرسائل من الأقدم للأحدث
        //                        .Select(m => new MessageDTO
        //                        {
        //                            Sender_Type = m.Sender_Type,
        //                            Text = m.Content,
        //                            Time = m.Created_At.UtcDateTime
        //                        }).ToList(),
        //            Last_Activity = conv.Last_Activity // لو محتاج تعرض آخر نشاط
        //        });
        //    }

        //    return result;
        //}






        public async Task<Conversation?> GetActiveConversationAsync(int userId)
        {
            return await _context.Conversations
                .Where(c => c.User_ID == userId && c.Is_Active)
                .OrderByDescending(c => c.Last_Activity)
                .FirstOrDefaultAsync();
        }







    }
}
