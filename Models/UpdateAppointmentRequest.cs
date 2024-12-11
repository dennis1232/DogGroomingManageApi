namespace DogGroomingAPI.Models
{
    public class UpdateAppointmentRequest
    {
        public DateTime AppointmentTime { get; set; }
        public int Duration { get; set; }
        public string PetSize { get; set; }
        public string PetName { get; set; }
    }
}