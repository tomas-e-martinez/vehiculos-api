using Microsoft.AspNetCore.Identity;
using vehiculos_api.Model;

namespace vehiculos_api.Service
{
    public class UsersService
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string HashPassword(User user, string password)
        {
            return _hasher.HashPassword(user, password);
        }

        public bool VerifyPassword(User user, string hashedPassword, string plainPassword)
        {
            var result = _hasher.VerifyHashedPassword(user, hashedPassword, plainPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
