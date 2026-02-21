using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Empath_AI.Model
{
    public class Conversation
    {

        [Key]
        [Required]
        public int Conversations_ID { get; set; }
        [ForeignKey("user")]
        [Required]
        public int User_ID { get; set; }
       // [ForeignKey("bot")]
        public int Bot_ID { get; set; }

        [Required]
        public string Title { get; set; }
        public DateTimeOffset Created_At { get; set; } = DateTime.UtcNow;
        public DateTimeOffset Last_Activity { get; set; } = DateTime.UtcNow;
        public bool Is_Archived { get; set; } = false;
        public User user { get; set; }
        public bool Is_Active { get; set; } = true;

        public ICollection<Message> messages { get; set; }
        public string? FirstMessage { get; set; }
        

        //public Bot bot{get;set;}
        //public Medical_Report medical_Report{get;set;} 

    }
}
