using Microsoft.EntityFrameworkCore;
using DogGroomingAPI.Models;

namespace DogGroomingAPI.Services
{
    public class AppointmentValidationService
    {
        private const int MIN_APPOINTMENT_DURATION = 30;
        private const int MAX_APPOINTMENT_DURATION = 90;
        private const int BUSINESS_HOURS_START = 8;
        private const int BUSINESS_HOURS_END = 18;

        private readonly DogGroomingDbContext _context;

        public AppointmentValidationService(DogGroomingDbContext context)
        {
            _context = context;
        }

        public bool IsValidDuration(int duration)
            => duration >= MIN_APPOINTMENT_DURATION && duration <= MAX_APPOINTMENT_DURATION;

        public bool IsValidBusinessHour(DateTime appointmentTime)
            => appointmentTime.Hour >= BUSINESS_HOURS_START && appointmentTime.Hour < BUSINESS_HOURS_END;

        public async Task<bool> HasConflict(DateTime appointmentTime, int duration, int? excludeAppointmentId = null)
        {
            var query = _context.Appointments.AsQueryable();

            if (excludeAppointmentId.HasValue)
                query = query.Where(a => a.Id != excludeAppointmentId.Value);

            return await query.AnyAsync(a =>
                a.AppointmentTime < appointmentTime.AddMinutes(duration) &&
                appointmentTime < a.AppointmentTime.AddMinutes(a.GroomingDuration)
            );
        }
    }
}
