using YourProject.Models;
using System.Threading.Tasks;

namespace YourProject.Services
{
    public interface IUamService
    {
        Task<User> LoginAsync(string email, string password);
        Task<bool> SignupAsync(string firstName, string lastName, string email, string password);
        Task<bool> UserExistsAsync(string email);
    }
}