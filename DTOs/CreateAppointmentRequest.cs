namespace DogGroomingAPI.DTOs
{
    public class CreateAppointmentRequest
    {
        public DateTime AppointmentTime { get; set; }
        public int Duration { get; set; }
        public string PetSize { get; set; }
        public string PetName { get; set; }
    }
}