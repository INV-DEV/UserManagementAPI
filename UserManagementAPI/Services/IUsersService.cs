using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.DTOs;
using UserManagementAPI.Model;

namespace UserManagementAPI.Services
{
    public interface IUsersService
    {
        Task<IEnumerable<User?>> GetAllUsersAsync(string? filterOn, string? filterQuery, string? sortBy, bool isAscending, int pageNumber, int pageSize, DateTime? dob);
        Task DeleteUserAsync(User user);
        Task<User> CreateNewUserAsync(User user);
        Task<User?> GetUserByIdAsync(Guid id);
        Task UpdateUserAsync(User updatedUser, User user);
    }
}