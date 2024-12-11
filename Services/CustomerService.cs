using DogGroomingAPI.Models;
using DogGroomingAPI.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.Data.SqlClient;

namespace DogGroomingAPI.Services;

public class CustomerService : ICustomerService
{
    private readonly DogGroomingDbContext _context;
    private readonly IConfiguration _configuration;

    public CustomerService(DogGroomingDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers.ToListAsync();
    }

    public async Task<Customer> RegisterAsync(RegisterRequest request)
    {
        // Check if username already exists
        if (await _context.Customers.AnyAsync(c => c.Username == request.Username))
        {
            throw new CustomerAlreadyExistsException("Username already exists");
        }

        // Create new customer
        var customer = new Customer
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Appointments = new List<Appointment>()
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return customer;
    }

    public async Task<string> LoginAsync(LoginRequest request)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Username == request.Username);

        if (customer == null || !BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        return GenerateJwtToken(customer);
    }

    public async Task<Customer> GetCustomerByIdAsync(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Appointments)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID {id} not found");
        }

        return customer;
    }

    public async Task<CurrentCustomerDto> GetCurrentCustomerAsync(int customerId)
    {
        var parameter = new SqlParameter("@CustomerId", customerId);

        var result = await _context.Set<CurrentCustomerDto>()
            .FromSqlRaw("EXEC [dbo].[GetCurrentCustomer] @CustomerId", parameter)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (result == null)
        {
            throw new NotFoundException($"Customer with ID {customerId} not found");
        }

        return result;
    }

    private string GenerateJwtToken(Customer customer)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
            throw new InvalidOperationException("JWT Key is not configured."));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Name, customer.Username),
                new Claim(ClaimTypes.Role, "Customer")
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
