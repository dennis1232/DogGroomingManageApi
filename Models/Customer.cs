using System;
using System.Collections.Generic;

namespace DogGroomingAPI.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; }
}
