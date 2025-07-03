using OnePlus.Models;
using System.Threading.Tasks;

namespace OnePlus.Services
{
    public interface IUamService
    {
        Task<User> LoginAsync(string email, string password);
        Task<bool> SignupAsync(string firstName, string lastName, string email, string password);
        Task<bool> UserExistsAsync(string email);
        Task<User> GetUserByIdAsync(int userId);
    }
}