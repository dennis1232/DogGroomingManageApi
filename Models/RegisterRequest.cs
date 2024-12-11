using System.ComponentModel.DataAnnotations;

namespace DogGroomingAPI.Models;

public class RegisterRequest
{
    [Required]
    [MinLength(5)]
    public string Username { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    [Required]
    [MaxLength(50)]
    public string FullName { get; set; }
}