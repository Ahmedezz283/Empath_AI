namespace Empath_AI.DTO.Device
{
    public class DeviceRegisterDTO
    {
        public string Name { get; set; }
        public string serial_number { get; set; }
        public int UserId { get; set; }
        public string? DeviceToken { get; set; }
    }
}
