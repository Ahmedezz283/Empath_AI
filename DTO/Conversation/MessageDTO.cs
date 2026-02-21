namespace Empath_AI.DTO.Conversation
{
    public class MessageDTO
    {
        public string Sender_Type { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public string username { get; set; }
        public int Conversation_ID { get; set; }
        public int? bot_id { get; set; }

        public string Text { get; set; }
        public DateTime Time { get; set; }

       // public DateTimeOffset Last_Activity { get; set; } = DateTime.UtcNow;
    }
}
