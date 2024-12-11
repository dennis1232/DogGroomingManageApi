public class AppointmentDTO
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string PetName { get; set; }
    public string PetSize { get; set; }
    public DateTime AppointmentTime { get; set; }
    public int GroomingDuration { get; set; }
    public DateTime CreatedAt { get; set; }
}