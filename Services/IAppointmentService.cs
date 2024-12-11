using DogGroomingAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DogGroomingAPI.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDTO>> GetAppointments(DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<AppointmentDTO>> GetCustomerAppointments(int customerId);
        Task<Appointment> GetAppointmentById(int appointmentId, int customerId);
        Task<bool> CancelAppointment(int appointmentId, int customerId);
        Task<Appointment> CreateAppointment(CreateAppointmentRequest request, int customerId);
        Task<Appointment> UpdateAppointment(int appointmentId, UpdateAppointmentRequest request, int customerId);
        Task<IEnumerable<DateTime>> GetAvailableTimes(DateTime date, int duration);
    }
}
