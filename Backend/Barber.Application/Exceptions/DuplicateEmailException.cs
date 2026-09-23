namespace Barber.Application.Exceptions;

public sealed class DuplicateEmailException()
    : Exception("Email is already registered.");
