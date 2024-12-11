using DogGroomingAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DogGroomingAPI.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetAppointmentsAsync(DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<Appointment>> GetCustomerAppointmentsAsync(int customerId);
        Task<Appointment> GetAppointmentByIdAsync(int appointmentId, int customerId);
        Task<bool> CancelAppointmentAsync(int appointmentId, int customerId);
        Task<Appointment> CreateAppointmentAsync(CreateAppointmentRequest request, int customerId);
        Task<Appointment> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentRequest request, int customerId);
        Task<IEnumerable<DateTime>> GetAvailableTimesAsync(DateTime date, int duration);
    }
}
