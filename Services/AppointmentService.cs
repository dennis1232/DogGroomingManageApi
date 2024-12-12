using DogGroomingAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DogGroomingAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly DogGroomingDbContext _context;
        private readonly AppointmentValidationService _validationService;

        public AppointmentService(
            DogGroomingDbContext context,
            AppointmentValidationService validationService)
        {
            _context = context;
            _validationService = validationService;
        }

        public async Task<IEnumerable<AppointmentDTO>> GetAppointments(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Appointments.Include(a => a.Customer).AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentTime >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(a => a.AppointmentTime <= toDate.Value);

            var appointments = await query.OrderBy(a => a.AppointmentTime).ToListAsync();
            return appointments.Select(a => MapToDTO(a));
        }

        public async Task<IEnumerable<AppointmentDTO>> GetCustomerAppointments(int customerId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Where(a => a.CustomerId == customerId)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            return appointments.Select(a => MapToDTO(a));
        }

        public async Task<Appointment> GetAppointmentById(int appointmentId, int customerId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found");

            if (appointment.CustomerId != customerId)
                throw new UnauthorizedAccessException();

            return appointment;
        }

        public async Task<Appointment> CreateAppointment(CreateAppointmentRequest request, int customerId)
        {
            if (!_validationService.IsValidDuration(request.Duration))
                throw new ValidationException("Invalid appointment duration");

            if (!_validationService.IsValidBusinessHour(request.AppointmentTime))
                throw new ValidationException("Invalid business hour");

            if (await _validationService.HasConflict(request.AppointmentTime, request.Duration))
                throw new ValidationException("Appointment conflicts with existing appointment");

            var appointment = new Appointment
            {
                CustomerId = customerId,
                AppointmentTime = request.AppointmentTime,
                GroomingDuration = request.Duration,
                PetSize = request.PetSize,
                PetName = request.PetName,
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<bool> CancelAppointment(int appointmentId, int customerId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);

            if (appointment == null)
                return false;

            if (appointment.CustomerId != customerId)
                throw new UnauthorizedAccessException();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Appointment> UpdateAppointment(int appointmentId, UpdateAppointmentRequest request, int customerId)
        {
            var appointment = await GetAppointmentById(appointmentId, customerId);

            if (!_validationService.IsValidDuration(request.Duration))
                throw new ValidationException("Invalid appointment duration");

            if (!_validationService.IsValidBusinessHour(request.AppointmentTime))
                throw new ValidationException("Invalid business hour");

            if (await _validationService.HasConflict(request.AppointmentTime, request.Duration, appointmentId))
                throw new ValidationException("Appointment conflicts with existing appointment");

            appointment.AppointmentTime = request.AppointmentTime;
            appointment.GroomingDuration = request.Duration;
            appointment.PetSize = request.PetSize;
            appointment.PetName = request.PetName;

            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<IEnumerable<DateTime>> GetAvailableTimes(DateTime date, int duration)
        {
            var businessStart = new DateTime(date.Year, date.Month, date.Day, BUSINESS_HOURS_START, 0, 0);
            var businessEnd = new DateTime(date.Year, date.Month, date.Day, BUSINESS_HOURS_END, 0, 0);
            var availableTimes = new List<DateTime>();

            var existingAppointments = await _context.Appointments
                .Where(a => a.AppointmentTime.Date == date.Date)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            var currentTime = businessStart;
            while (currentTime.AddMinutes(duration) <= businessEnd)
            {
                var hasConflict = await _validationService.HasConflict(currentTime, duration);
                if (!hasConflict)
                {
                    availableTimes.Add(currentTime);
                }
                currentTime = currentTime.AddMinutes(30);
            }

            return availableTimes;
        }

        private AppointmentDTO MapToDTO(Appointment appointment)
        {
            return new AppointmentDTO
            {
                Id = appointment.Id,
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer?.FullName,
                PetName = appointment.PetName,
                PetSize = appointment.PetSize,
                AppointmentTime = appointment.AppointmentTime,
                GroomingDuration = appointment.GroomingDuration,
                CreatedAt = appointment.CreatedAt
            };
        }

        private const int BUSINESS_HOURS_START = 8;
        private const int BUSINESS_HOURS_END = 18;
    }
}
