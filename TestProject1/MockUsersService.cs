using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementAPI.Database;
using UserManagementAPI.DTOs;
using UserManagementAPI.Model;
using UserManagementAPI.Services;

namespace TestProject1
{
    public class MockUsersService : IUsersService
    {
        public User? _user = null;
        public IEnumerable<User>? _users = null;

        public async Task<User?> CreateNewUserAsync(UserManagementAPI.Model.User user)
        {
            if (user != null) {
                return user;
            }
            return null;
        }

        public async Task DeleteUserAsync(User user)
        {
        }

        public async Task<IEnumerable<User>?> GetAllUsersAsync(string? filterOn, string? filterQuery, string? sortBy, bool isAscending, int pageNumber, int pageSize, DateTime? dob)
        {
            if (_users != null)
            {
                return _users;
            }
            return new List<User>();
        }

        public Task<IEnumerable<User?>?> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<UserManagementAPI.Model.User?> GetUserByIdAsync(Guid id)
        {
            if(_user != null)
            {
                return _user;
            }
            return null;
        }

        public UserContext GetUserContext()
        {
            return null;
            //throw new NotImplementedException();
        }

        public async Task UpdateUserAsync(UserManagementAPI.Model.User updatedUser, User user)
        {
            if (user != null)
            {
            }
        }
    }
}
