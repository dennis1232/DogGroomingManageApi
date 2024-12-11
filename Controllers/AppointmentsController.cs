using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DogGroomingAPI.Models;
using Microsoft.Data.SqlClient;
using DogGroomingAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace DogGroomingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Not authenticated");

                var customerId = int.Parse(userId);
                var appt = await _appointmentService.CreateAppointment(request, customerId);
                return Ok(new { appointment = appt });
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log error here
                return StatusCode(500, "Failed to create appointment: " + ex.Message);
            }
        }

        [Authorize]
        [HttpGet("available-times")]
        public async Task<IActionResult> GetAvailableTimes(DateTime date, int duration)
        {
            try
            {
                var times = await _appointmentService.GetAvailableTimes(date, duration);
                return Ok(times);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Error getting available times");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var appts = await _appointmentService.GetAppointments(fromDate, toDate);

                return Ok(appts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to get appointments: " + ex.Message);
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetMyAppointments()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var customerId = int.Parse(userId);
                var myAppts = await _appointmentService.GetCustomerAppointments(customerId);

                return Ok(myAppts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error retrieving appointments: " + ex.Message);
            }
        }

        [HttpDelete("{appointmentId}")]
        [Authorize]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var customerId = int.Parse(userId);
                var cancelled = await _appointmentService.CancelAppointment(appointmentId, customerId);

                if (!cancelled)
                    return NotFound("Appointment not found");

                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Failed to cancel: " + ex.Message);
            }
        }

        [HttpPut("{appointmentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateAppointment(
            int appointmentId,
            [FromBody] UpdateAppointmentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var customerId = int.Parse(userId);
                var updated = await _appointmentService.UpdateAppointment(appointmentId, request, customerId);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Appointment not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Update failed: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAppointment(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var customerId = int.Parse(userId);
                var appt = await _appointmentService.GetAppointmentById(id, customerId);
                return Ok(appt);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Appointment not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error retrieving appointment: " + ex.Message);
            }
        }
    }
}
