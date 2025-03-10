using dotSocialNetwork.Server.Models;
namespace dotSocialNetwork.Server.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        int GetCurrentUserId();
        Task<List<User>> SearchUsersAsync(string query);
    }
}
