//using Microsoft.Extensions.Hosting;
//using Microsoft.EntityFrameworkCore;
//using Empath_AI.Data;

//public class DeleteOldConversationsService : BackgroundService
//{
//    private readonly IServiceProvider _serviceProvider;

//    public DeleteOldConversationsService(IServiceProvider serviceProvider)
//    {
//        _serviceProvider = serviceProvider;
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            using (var scope = _serviceProvider.CreateScope())
//            {
//                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//                // Determine the limit date (30 days ago)
//                var limitDate = DateTimeOffset.UtcNow.AddDays(-30);

//                // Fetch old conversations
//                var oldConversations = await db.Conversations
//                    .Where(c => c.Last_Activity < limitDate && c.Conversations_ID == 101)
//                    .ToListAsync(stoppingToken);

//                foreach (var convo in oldConversations)
//                {
//                    // Delete all messages related to this conversation
//                    var msgs = db.Messages.Where(m => m.Conversation_ID == convo.Conversations_ID);
//                    db.Messages.RemoveRange(msgs);

//                    // Delete the conversation itself
//                    db.Conversations.Remove(convo);
//                }

//                await db.SaveChangesAsync(stoppingToken);
//            }

//            // Run cleanup once per day
//            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
//        }
//    }
//}
