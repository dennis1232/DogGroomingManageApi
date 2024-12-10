using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;



using DogGroomingAPI.Models; // Replace with your actual namespace

namespace DogGroomingAPI.Controllers // Replace with your actual namespace
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly DogGroomingDbContext _context;

        public AppointmentsController(DogGroomingDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            // if (!ModelState.IsValid)
            //     return BadRequest(ModelState);

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User is not authenticated." });

            var customer = await _context.Customers.FindAsync(int.Parse(userId));
            if (customer == null)
                return NotFound(new { message = "Customer not found." });

            // Check for appointment conflicts
            var isConflict = await _context.Appointments.AnyAsync(a =>
                a.AppointmentTime < request.AppointmentTime.AddMinutes(request.Duration) &&
                request.AppointmentTime < a.AppointmentTime.AddMinutes(a.GroomingDuration)
            );

            if (isConflict)
                return BadRequest(new { message = "Appointment conflicts with an existing appointment." });

            // Create new appointment
            var appointment = new Appointment
            {
                CustomerId = customer.Id,
                AppointmentTime = request.AppointmentTime,
                GroomingDuration = request.Duration,
                PetSize = request.PetSize,
                PetName = request.PetName,
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { appointment });
        }


        [Authorize]
        [HttpGet("available-times")]
        public async Task<IActionResult> GetAvailableTimes(DateTime date, int duration)
        {
            var appointments = await _context.Appointments
                .Where(a => a.AppointmentTime.Date == date.Date)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            var startOfDay = date.Date.AddHours(8); // Start at 8:00 AM
            var endOfDay = date.Date.AddHours(18); // End at 6:00 PM
            var availableTimes = new List<DateTime>();
            var currentSlot = startOfDay;

            while (currentSlot.AddMinutes(duration) <= endOfDay)
            {
                bool isConflict = appointments.Any(a =>
                    currentSlot < a.AppointmentTime.AddMinutes(a.GroomingDuration) &&
                    currentSlot.AddMinutes(duration) > a.AppointmentTime
                );

                if (!isConflict)
                {
                    availableTimes.Add(currentSlot);
                }

                currentSlot = currentSlot.AddMinutes(15); // Increment by 15 minutes
            }

            return Ok(availableTimes);
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            return Ok(await _context.Appointments.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return Ok(appointment);
        }
    }


    public class CreateAppointmentRequest
    {
        public DateTime AppointmentTime { get; set; }

        public int Duration { get; set; }

        public string PetSize { get; set; }
        public string PetName { get; set; }
    }

}
