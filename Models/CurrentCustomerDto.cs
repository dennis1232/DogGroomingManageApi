namespace DogGroomingAPI.Models
{
    public class CurrentCustomerDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}
