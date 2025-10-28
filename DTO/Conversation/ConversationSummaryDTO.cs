namespace Empath_AI.DTO.Conversation
{
    public class ConversationSummaryDTO
    {
        public int Conversations_ID { get; set; } //?
        public string Title { get; set; }
        public DateTimeOffset Last_Activity { get; set; } = DateTime.UtcNow;
        // public string LastMessage { get; set; }



    }
}
