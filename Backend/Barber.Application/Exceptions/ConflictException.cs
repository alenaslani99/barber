namespace Barber.Application.Exceptions;

/// <summary>
/// The request clashes with existing state (slug taken, owner already set, ...). Maps to 409.
/// </summary>
public sealed class ConflictException(string message) : Exception(message);
