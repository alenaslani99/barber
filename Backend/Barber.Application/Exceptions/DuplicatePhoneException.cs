namespace Barber.Application.Exceptions;

public sealed class DuplicatePhoneException(string phone)
    : Exception($"Phone '{phone}' is already registered.");
