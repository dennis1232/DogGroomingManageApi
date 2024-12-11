using DogGroomingAPI.Models;
using DogGroomingAPI.DTOs;
using DogGroomingAPI.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogGroomingAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly DogGroomingDbContext _context;

        public AppointmentService(DogGroomingDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment> CreateAppointmentAsync(int customerId, CreateAppointmentRequest request)
        {
            // Check for appointment conflicts
            var isConflict = await _context.Appointments
                .AnyAsync(a =>
                    a.AppointmentTime < request.AppointmentTime.AddMinutes(request.Duration) &&
                    request.AppointmentTime < a.AppointmentTime.AddMinutes(a.GroomingDuration)
                );

            if (isConflict)
                throw new AppointmentConflictException("The requested time slot conflicts with an existing appointment.");

            var appointment = new Appointment
            {
                CustomerId = customerId,
                AppointmentTime = request.AppointmentTime,
                GroomingDuration = request.Duration,
                PetSize = request.PetSize,
                PetName = request.PetName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<IEnumerable<DateTime>> GetAvailableTimesAsync(DateTime date, int duration)
        {
            var appointments = await _context.Appointments
                .Where(a => a.AppointmentTime.Date == date.Date)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            var startOfDay = date.Date.AddHours(8); // Start at 8:00 AM
            var endOfDay = date.Date.AddHours(18);  // End at 6:00 PM
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

                currentSlot = currentSlot.AddMinutes(15); // 15-minute intervals
            }

            return availableTimes;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Appointments
                .Include(a => a.Customer)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentTime.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(a => a.AppointmentTime.Date <= toDate.Value.Date);

            var appointments = await query
                .OrderBy(a => a.AppointmentTime)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentTime = a.AppointmentTime,
                    GroomingDuration = a.GroomingDuration,
                    PetSize = a.PetSize,
                    PetName = a.PetName,
                    CustomerId = a.CustomerId,
                    CreatedAt = a.CreatedAt,
                    CustomerName = a.Customer.FullName
                })
                .ToListAsync();

            return appointments;
        }

        public async Task<IEnumerable<AppointmentDto>> GetCustomerAppointmentsAsync(int customerId)
        {
            return await _context.Appointments
                .Include(a => a.Customer)
                .Where(a => a.CustomerId == customerId)
                .OrderBy(a => a.AppointmentTime)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentTime = a.AppointmentTime,
                    GroomingDuration = a.GroomingDuration,
                    PetSize = a.PetSize,
                    PetName = a.PetName,
                    CustomerId = a.CustomerId,
                    CreatedAt = a.CreatedAt,
                    CustomerName = a.Customer.FullName
                })
                .ToListAsync();
        }

        public async Task<AppointmentDto> GetAppointmentByIdAsync(int appointmentId, int customerId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                throw new NotFoundException("Appointment not found");

            if (appointment.CustomerId != customerId)
                throw new UnauthorizedAccessException();

            return new AppointmentDto
            {
                Id = appointment.Id,
                AppointmentTime = appointment.AppointmentTime,
                GroomingDuration = appointment.GroomingDuration,
                PetSize = appointment.PetSize,
                PetName = appointment.PetName,
                CustomerId = appointment.CustomerId,
                CreatedAt = appointment.CreatedAt,
                CustomerName = appointment.Customer.FullName
            };
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, int customerId)
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

        public async Task<Appointment> UpdateAppointmentAsync(int appointmentId, int customerId, UpdateAppointmentRequest request)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);

            if (appointment == null)
                throw new NotFoundException("Appointment not found");

            if (appointment.CustomerId != customerId)
                throw new UnauthorizedAccessException();

            var isConflict = await _context.Appointments
                .Where(a => a.Id != appointmentId)
                .AnyAsync(a =>
                    a.AppointmentTime < request.AppointmentTime.AddMinutes(request.Duration) &&
                    request.AppointmentTime < a.AppointmentTime.AddMinutes(a.GroomingDuration)
                );

            if (isConflict)
                throw new AppointmentConflictException("The requested time slot conflicts with an existing appointment.");

            appointment.AppointmentTime = request.AppointmentTime;
            appointment.GroomingDuration = request.Duration;
            appointment.PetSize = request.PetSize;
            appointment.PetName = request.PetName;

            await _context.SaveChangesAsync();
            return appointment;
        }
    }
}
