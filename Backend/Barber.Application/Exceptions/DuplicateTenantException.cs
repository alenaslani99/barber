namespace Barber.Application.Exceptions;

public sealed class DuplicateTenantException(string message) : Exception(message);
