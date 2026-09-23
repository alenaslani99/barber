namespace Barber.Application.Exceptions;

public sealed class DuplicatePhoneException()
    : Exception("Phone is already registered.");
