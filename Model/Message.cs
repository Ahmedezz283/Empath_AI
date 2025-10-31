using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Empath_AI.Model
{
    public class Message
    {
        [Key]
        public int ID { get; set; }     
        public int? User_ID { get; set; }    
        public int? Bot_ID { get; set; }     
        public int Device_ID { get; set; }  
        public int Conversation_ID { get; set; } 

        public string Content { get; set; }
        public string Sender_Type { get; set; } 
        public string Message_Type { get; set; } 
        public DateTimeOffset Created_At { get; set; } = DateTimeOffset.UtcNow;
    }
}
