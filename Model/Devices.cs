using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Empath_AI.Model
{
    public class Devices
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string serial_number { get; set; }
        [Required]
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public string? DeviceToken { get; set; } 
        public bool IsActive { get; set; } = true;
        public DateTimeOffset Created_At { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset Last_Active { get; set; } = DateTime.UtcNow;

    }
}
