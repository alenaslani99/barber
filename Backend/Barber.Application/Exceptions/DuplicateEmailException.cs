namespace Barber.Application.Exceptions;

public sealed class DuplicateEmailException(string email)
    : Exception($"Email '{email}' is already registered.");
