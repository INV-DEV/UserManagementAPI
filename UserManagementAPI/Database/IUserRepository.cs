using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Model;

namespace UserManagementAPI.Database
{
    public interface IUserRepository : IDisposable
    {
        Task<IEnumerable<User>?> GetCompleteUsersAsync();
        Task DeleteUserAsync(User id);
        Task InsertUserAsync(User user);
        Task SaveAsync();
        Task<User?> GetUserByIdAsync(Guid userId);
        Task UpdateUserAsync(User user);

        IEnumerable<User> GetUsers();
        User GetUserByID(Guid userId);
        void InsertUser(User user);
        void DeleteUser(User user);
        void UpdateUser(User user);
        void Save();
    }
}