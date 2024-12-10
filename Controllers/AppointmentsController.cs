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
        [Authorize]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User is not authenticated." });

            var customerId = int.Parse(userId);
            var customer = await _context.Customers.FindAsync(customerId);
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
        [Authorize]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.Customer)
                    .AsQueryable();

                // Apply date filters if provided
                if (fromDate.HasValue)
                    query = query.Where(a => a.AppointmentTime.Date >= fromDate.Value.Date);

                if (toDate.HasValue)
                    query = query.Where(a => a.AppointmentTime.Date <= toDate.Value.Date);

                var appointments = await query
                    .OrderBy(a => a.AppointmentTime)
                    .ToListAsync();

                var result = appointments.Select(a => new
                {
                    a.Id,
                    a.AppointmentTime,
                    a.GroomingDuration,
                    a.PetSize,
                    a.PetName,
                    a.CreatedAt,
                    a.CustomerId,
                    CustomerName = a.Customer.FullName
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving appointments.", error = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetMyAppointments()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                var userIdInt = int.Parse(userId);  // Parse string to int

                var appointments = await _context.Appointments
                    .Include(a => a.Customer)
                    .Where(a => a.Customer.Id == userIdInt)
                    .OrderBy(a => a.AppointmentTime)
                    .ToListAsync();

                var result = appointments.Select(a => new
                {
                    a.Id,
                    a.AppointmentTime,
                    a.GroomingDuration,
                    a.PetSize,
                    a.PetName,
                    a.CustomerId,
                    a.CreatedAt,
                    CustomerName = a.Customer.FullName
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving your appointments.", error = ex.Message });
            }
        }


        [HttpDelete]
        [Route("{appointmentId}")]
        [Authorize]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var customerId = int.Parse(userId);

            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return NotFound();

            if (appointment.CustomerId != customerId)
            {
                return Forbid();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{appointmentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateAppointment(int appointmentId, [FromBody] UpdateAppointmentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var customerId = int.Parse(userId);

            var existingAppointment = await _context.Appointments.FindAsync(appointmentId);
            if (existingAppointment == null) return NotFound();

            if (existingAppointment.CustomerId != customerId)
            {
                return Forbid();
            }

            // Check for appointment conflicts
            var isConflict = await _context.Appointments
                .Where(a => a.Id != appointmentId)
                .AnyAsync(a =>
                    a.AppointmentTime < request.AppointmentTime.AddMinutes(request.Duration) &&
                    request.AppointmentTime < a.AppointmentTime.AddMinutes(a.GroomingDuration)
                );

            if (isConflict)
                return BadRequest(new { message = "Appointment conflicts with an existing appointment." });

            // Update appointment properties
            existingAppointment.AppointmentTime = request.AppointmentTime;
            existingAppointment.GroomingDuration = request.Duration;
            existingAppointment.PetSize = request.PetSize;
            existingAppointment.PetName = request.PetName;

            await _context.SaveChangesAsync();
            return Ok(existingAppointment);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAppointment(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();
                var customerId = int.Parse(userId);

                var appointment = await _context.Appointments
                    .Include(a => a.Customer)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null) return NotFound();

                if (appointment.CustomerId != customerId)
                {
                    return Forbid();
                }

                var result = new
                {
                    appointment.Id,
                    appointment.AppointmentTime,
                    appointment.GroomingDuration,
                    appointment.PetSize,
                    appointment.PetName,
                    appointment.CustomerId,
                    appointment.CreatedAt,
                    CustomerName = appointment.Customer.FullName
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the appointment.", error = ex.Message });
            }
        }


    }


    public class CreateAppointmentRequest
    {
        public DateTime AppointmentTime { get; set; }

        public int Duration { get; set; }

        public string PetSize { get; set; }
        public string PetName { get; set; }
    }

    public class UpdateAppointmentRequest
    {
        public DateTime AppointmentTime { get; set; }
        public int Duration { get; set; }
        public string PetSize { get; set; }
        public string PetName { get; set; }
    }

}
