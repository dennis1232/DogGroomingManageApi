using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace DogGroomingAPI.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        [JsonIgnore]
        public Customer Customer { get; set; }

        public string PetName { get; set; }

        public string PetSize { get; set; }

        public DateTime AppointmentTime { get; set; }

        public int GroomingDuration { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
