namespace Empath_AI.DTO.Conversation
{
    public class ConversationDTO
    {
        public string Title { get; set; }
        public DateTime Created_At { get; set; }
        public DateTimeOffset Last_Activity { get; set; }
        public int userid { get; set; }
        //public string FirstMessage { get; set; }

        
    }
}
