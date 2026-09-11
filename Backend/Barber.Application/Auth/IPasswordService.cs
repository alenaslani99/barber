using Barber.Domain;

namespace Barber.Application.Auth;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string passwordHash, string password);
}
