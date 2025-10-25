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
        public int? Bot_ID { get; set; }
        public int? Medical_ID { get; set; }
        [Required]
        public string Title { get; set; }
        public DateTime Created_At { get;set;}
        public DateTime Last_Activity { get; set; }
        public User user { get; set; }
       
    }
}
