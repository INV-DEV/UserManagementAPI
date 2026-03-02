using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementAPI.DTOs;
using UserManagementAPI.Services;

namespace TestProject1
{
    public class MockUsersService : IUsersService
    {
        public ActionResult CreateNewUser(UserManagementAPI.Model.User user)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> CreateNewUserAsync(UserManagementAPI.Model.User user)
        {
            throw new NotImplementedException();
        }

        public ActionResult DeleteUser(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> DeleteUserAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public ActionResult<IEnumerable<UserManagementAPI.Model.User>> GetAllUsers(string? filterOn, string filterQuery, string? sortBy, bool isAscending, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult<IEnumerable<GetAllUsersResponse>>> GetAllUsersAsync(string? filterOn, string? filterQuery, string? sortBy, bool isAscending, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public ActionResult<UserManagementAPI.Model.User> GetUserById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult<UserManagementAPI.Model.User>> GetUserByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public ActionResult UpdateUser(UserManagementAPI.Model.User updatedUser, Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> UpdateUserAsync(UserManagementAPI.Model.User updatedUser, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
