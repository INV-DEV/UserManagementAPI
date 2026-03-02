using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.DTOs;
using UserManagementAPI.Model;

namespace UserManagementAPI.Services
{
    public interface IUsersService
    {
        Task<ActionResult<IEnumerable<GetAllUsersResponse>>> GetAllUsersAsync(string? filterOn, string? filterQuery, string? sortBy, bool isAscending, int pageNumber, int pageSize);
        Task<ActionResult> DeleteUserAsync(Guid id);
        Task<ActionResult> CreateNewUserAsync(User user);
        Task<ActionResult<User>> GetUserByIdAsync(Guid id);
        Task<ActionResult> UpdateUserAsync(User updatedUser, Guid id);

        ActionResult CreateNewUser(User user);
        ActionResult DeleteUser(Guid id);
        ActionResult<IEnumerable<User>> GetAllUsers(string? filterOn, string filterQuery, string? sortBy, bool isAscending, int pageNumber, int pageSize);
        ActionResult<User> GetUserById(Guid id);
        ActionResult UpdateUser(User updatedUser, Guid id);
    }
}