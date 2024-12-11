using System;

namespace DogGroomingAPI.Models
{
    public class CreateAppointmentRequest
    {
        public DateTime AppointmentTime { get; set; }
        public int Duration { get; set; }
        public string PetSize { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
    }
}