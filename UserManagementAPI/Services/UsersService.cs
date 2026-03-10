using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using UserManagementAPI.Controllers;
using UserManagementAPI.Database;
using UserManagementAPI.DTOs;
using UserManagementAPI.Extensions;
using UserManagementAPI.Model;

namespace UserManagementAPI.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUserRepository _userRepository;

        public UsersService(UserContext context)
        {
            _userRepository = new UserRepository(context);
        }

        public UserContext GetUserContext()
        {
            return _userRepository.GetUserContext();
        }

        #region Async

        public async Task DeleteUserAsync(User user)
        {
            await _userRepository.DeleteUserAsync(user);
            await _userRepository.SaveAsync();
        }

        public async Task<IEnumerable<User>?> GetAllUsersAsync(
            string? filterOn, 
            string? filterQuery, 
            string? sortBy, 
            bool isAscending, 
            int pageNumber, 
            int pageSize,
            DateTime? dob)
        {
            var usersQuery = await _userRepository.GetCompleteUsersAsync();
            if (usersQuery == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    usersQuery = usersQuery.Where(user => user.Name.Contains(filterQuery));

                if (filterOn.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    usersQuery = usersQuery.Where(user => user.Email.Contains(filterQuery));

                if (filterOn.Equals("DateOfBirth", StringComparison.OrdinalIgnoreCase))
                    usersQuery = usersQuery.Where(user => user.DateOfBirth == dob);
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    usersQuery = isAscending ? usersQuery.OrderBy(user => user.Name) : usersQuery.OrderByDescending(user => user.Name);

                if (sortBy.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    usersQuery = isAscending ? usersQuery.OrderBy(user => user.Email) : usersQuery.OrderByDescending(user => user.Email);

                if (sortBy.Equals("DateOfBirth", StringComparison.OrdinalIgnoreCase))
                    usersQuery = isAscending ? usersQuery.OrderBy(user => user.DateOfBirth) : usersQuery.OrderByDescending(user => user.DateOfBirth);
            }
            
            var getAllArgs = new GetAllArgs();
            getAllArgs.PageNumber = pageNumber;
            getAllArgs.PageSize = pageSize;

            var paginatedUsersQuery = usersQuery.AsQueryable().Paginate(getAllArgs).ToList();

            return paginatedUsersQuery;
        }

        public async Task<User> CreateNewUserAsync(User user)
        {
            //user.CreatedAt = DateTime.Now;
            //user.UpdatedAt = DateTime.Now;
            await _userRepository.InsertUserAsync(user);
            await _userRepository.SaveAsync();
            return user;
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task UpdateUserAsync(User updatedUser, User user)
        {
            user.DateOfBirth = updatedUser.DateOfBirth;
            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            //user.Password = updatedUser.Password;
            //user.UpdatedAt = DateTime.Now;

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveAsync();
        }

        #endregion

        private static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age)) age--;

            return age;
        }

        private List<UserModel> GetUsersModel(IEnumerable<User> users)
        {
            var usersModelList = new List<UserModel>();
            foreach (var user in users)
                usersModelList.Add(new()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                });

            return usersModelList;
        }

        private UserModel GetUserModel(User user)
            =>
                new UserModel()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

        public async Task<IEnumerable<User?>?> GetAllUsersAsync()
        {
            return await _userRepository.GetCompleteUsersAsync();
        }
    }
}
