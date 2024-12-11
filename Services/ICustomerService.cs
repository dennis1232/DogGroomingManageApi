using DogGroomingAPI.Models;
using System.Threading.Tasks;

namespace DogGroomingAPI.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer> RegisterAsync(RegisterRequest request);
        Task<string> LoginAsync(LoginRequest request);
        Task<Customer> GetCustomerByIdAsync(int id);
        Task<CurrentCustomerDto> GetCurrentCustomerAsync(int customerId);
    }
}
