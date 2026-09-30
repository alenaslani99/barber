using System.Security.Cryptography;

namespace Barber.Application.Auth;

/// <summary>
/// Generates initial passwords for accounts the admin creates (owner, barbers).
/// </summary>
public static class PasswordGenerator
{
    // No look-alikes (0/O, 1/l/I) so a password read off the screen types correctly.
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnpqrstuvwxyz";
    private const string Digits = "23456789";
    private const string All = Upper + Lower + Digits;

    public static string Generate(int length = 16)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 8);

        char[] chars = new char[length];
        chars[0] = Pick(Upper);
        chars[1] = Pick(Lower);
        chars[2] = Pick(Digits);
        for (int i = 3; i < length; i++)
            chars[i] = Pick(All);

        RandomNumberGenerator.Shuffle(chars.AsSpan());
        return new string(chars);
    }

    private static char Pick(string alphabet) => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
}
