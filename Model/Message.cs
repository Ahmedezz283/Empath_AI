using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Empath_AI.Model
{
    public class Message
    {
        [Key]
        public int ID { get; set; }     
        public int? User_ID { get; set; }    
        public int? Bot_ID { get; set; }     
        public int Device_ID { get; set; }

        [ForeignKey("conversation")]
        public int Conversation_ID { get; set; } 

        public string Content { get; set; }
        public string Sender_Type { get; set; } 
        public string Message_Type { get; set; } 
        public DateTimeOffset Created_At { get; set; } = DateTimeOffset.UtcNow;
        public Conversation conversation{get;set;}


    }
}
