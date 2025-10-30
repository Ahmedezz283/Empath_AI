using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Empath_AI.DTO.Device
{
    public class DeviceDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string serial_number { get; set; }
        public int UserId { get; set; }
        public string? DeviceToken { get; set; }
    }
}
