namespace Barber.Application.Exceptions;

public sealed class BookingConflictException(string message) : Exception(message);
