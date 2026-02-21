namespace Empath_AI.DTO.Conversation
{
    public class ConversationContentDTO
    {
        public int ConversationId { get; set; }
        public string Title { get; set; }
        public List<MessageDTO> Messages { get; set; }
        public DateTimeOffset Last_Activity { get; set; }

    }
}
