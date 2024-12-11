namespace DogGroomingAPI.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class AppointmentConflictException : Exception
    {
        public AppointmentConflictException(string message) : base(message) { }
    }
}
