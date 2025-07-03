using OnePlus.Data;
using OnePlus.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net;

namespace OnePlus.Services
{
    public class UamService : IUamService
    {
        private readonly AppDbContext _context;

        public UamService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            // CORRECTED LINE: Use the full, explicit class name.
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return isPasswordValid ? user : null;
        }

        public async Task<bool> SignupAsync(string firstName, string lastName, string email, string password)
        {
            if (await UserExistsAsync(email))
            {
                return false; // User already exists
            }

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                // CORRECTED LINE: Use the full, explicit class name.
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 12),
                Role = UserRole.Client
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }
    }
}
