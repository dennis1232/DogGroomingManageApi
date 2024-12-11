using DogGroomingAPI.Models;
using System.Threading.Tasks;

namespace DogGroomingAPI.Services
{
    public interface ICustomerService
    {
        Task<Customer> RegisterAsync(RegisterRequest request);
        Task<string> LoginAsync(LoginRequest request);
        Task<Customer> GetCustomerByIdAsync(int id);

    }
}
