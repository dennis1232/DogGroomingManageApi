using DogGroomingAPI.Models;
using DogGroomingAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DogGroomingAPI.Services
{
    public interface IAppointmentService
    {
        Task<Appointment> CreateAppointmentAsync(int customerId, CreateAppointmentRequest request);
        Task<IEnumerable<DateTime>> GetAvailableTimesAsync(DateTime date, int duration);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsAsync(DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<AppointmentDto>> GetCustomerAppointmentsAsync(int customerId);
        Task<AppointmentDto> GetAppointmentByIdAsync(int appointmentId, int customerId);
        Task<bool> CancelAppointmentAsync(int appointmentId, int customerId);
        Task<Appointment> UpdateAppointmentAsync(int appointmentId, int customerId, UpdateAppointmentRequest request);
    }
}
