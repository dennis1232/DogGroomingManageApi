namespace DogGroomingAPI.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public DateTime AppointmentTime { get; set; }
        public int GroomingDuration { get; set; }
        public string PetSize { get; set; }
        public string PetName { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; }
    }
}