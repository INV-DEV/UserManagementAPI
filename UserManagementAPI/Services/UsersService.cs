using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<UsersController> _logger;
        private readonly IUserRepository _userRepository;

        public UsersService(ILogger<UsersController> logger, UserContext context)
        {
            _logger = logger;
            _userRepository = new UserRepository(context);
        }

        #region Async

        public async Task<ActionResult> DeleteUserAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult("Guid cannot be empty.");
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return new NotFoundObjectResult(new { Message = $"User with ID {id} not found." });
            }

            await _userRepository.DeleteUserAsync(user);
            _userRepository.Save();

            return new OkObjectResult(new { Message = "User deleted successfully." });
        }

        public async Task<ActionResult<IEnumerable<GetAllUsersResponse>?>> GetAllUsersAsync(
            string? filterOn, 
            string? filterQuery, 
            string? sortBy, 
            bool isAscending, 
            int pageNumber, 
            int pageSize)
        {
            var usersQuery = await _userRepository.GetCompleteUsersAsync();
            
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    usersQuery = usersQuery.Where(user => user.Name.Contains(filterQuery));

                if (filterOn.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    usersQuery = usersQuery.Where(user => user.Email.Contains(filterQuery));

                if (filterOn.Equals("DateOfBirth", StringComparison.OrdinalIgnoreCase))
                {
                    if (!DateTime.TryParse(filterQuery.ToLower(), out DateTime dob))
                    {
                        return new BadRequestObjectResult("DateOfBirth could not be parsed. Please provide a valid datetime.");
                    }
                    usersQuery = usersQuery.Where(user => user.DateOfBirth == dob);
                }
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    usersQuery = isAscending ? usersQuery.OrderBy(emp => emp.Name) : usersQuery.OrderByDescending(emp => emp.Name);

                if (sortBy.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    usersQuery = isAscending ? usersQuery.OrderBy(emp => emp.Email) : usersQuery.OrderByDescending(emp => emp.Email);

                if (sortBy.Equals("DateOfBirth", StringComparison.OrdinalIgnoreCase))
                {
                    usersQuery = isAscending ? usersQuery.OrderBy(emp => emp.DateOfBirth) : usersQuery.OrderByDescending(emp => emp.DateOfBirth);
                }
            }
            
            var getAllArgs = new GetAllArgs();
            getAllArgs.PageNumber = pageNumber;
            getAllArgs.PageSize = pageSize;

            var paginatedUsersQuery = usersQuery.AsQueryable().Paginate(getAllArgs).ToList();

            List<GetAllUsersResponse> response = new List<GetAllUsersResponse>();
            var usersDto = new List<GetAllUsersResponse>();
            foreach (var user in paginatedUsersQuery)
            {
                usersDto.Add(
                    new GetAllUsersResponse()
                    {
                        Id = user.Id,
                        DateOfBirth = user.DateOfBirth,
                        Email = user.Email,
                        Name = user.Name,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    });
            }

            return usersDto;
        }

        public async Task<ActionResult> CreateNewUserAsync(User user)
        {
            if (user == null)
            {
                return new BadRequestObjectResult("User cannot be empty.");
            }

            if (user.Id == Guid.Empty)
            {
                return new BadRequestObjectResult("User ID cannot be empty.");
            }

            if (string.IsNullOrEmpty(user.Name))
            {
                return new BadRequestObjectResult("User Name cannot be empty.");
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                return new BadRequestObjectResult("User Email cannot be empty.");
            }

            var age = CalculateAge(user.DateOfBirth);
            if (age < 18)
            {
                return new BadRequestObjectResult("User must be at least 18 years old.");
            }

            if (await _userRepository.GetUserByIdAsync(user.Id) != null)
            {
                return new BadRequestObjectResult("User already exists.");
            }

            await _userRepository.InsertUserAsync(user);
            await _userRepository.SaveAsync();
            return new CreatedAtActionResult("GetUserById", "Users", new { id = user.Id }, user);
        }

        public async Task<ActionResult<User>> GetUserByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult(new { Message = $"ID cannot be empty." });
            }

            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return new NotFoundObjectResult(new { Message = $"User with ID {id} not found." });
            }
            return new OkObjectResult(user);
        }

        public async Task<ActionResult> UpdateUserAsync(User updatedUser, Guid id)
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult("ID cannot be empty.");
            }

            if (id != updatedUser.Id)
                return new BadRequestObjectResult(new { Message = "User ID mismatch." });

            var age = CalculateAge(updatedUser.DateOfBirth);
            if (age < 18)
            {
                return new BadRequestObjectResult("User must be at least 18 years old.");
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return new NotFoundObjectResult(new { Message = $"User with ID {id} not found." });
            }

            user.DateOfBirth = updatedUser.DateOfBirth;
            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.UpdatedAt = DateTime.Now;

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveAsync();

            return new OkObjectResult(new { Message = "User updated successfully.", User = user });
        }

        #endregion

        private static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age)) age--;

            return age;
        }

        #region Non-Async

        public ActionResult<User> GetUserById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult("Guid cannot be empty.");
            }

            var user = _userRepository.GetUserByID(id);

            if (user == null)
            {
                return new NotFoundObjectResult(new { Message = $"User with ID {id} not found." });
            }
            return new OkObjectResult(user);
        }

        public ActionResult<IEnumerable<User>> GetAllUsers(string? filterOn, string filterQuery, string? sortBy, bool isAscending, int pageNumber, int pageSize)
        {
            var usersQuery = _userRepository.GetUsers();

            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    usersQuery = usersQuery.Where(user => user.Name.Contains(filterQuery));

                if (filterOn.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    usersQuery = usersQuery.Where(user => user.Email.Contains(filterQuery));

                if (filterOn.Equals("DateOfBirth", StringComparison.OrdinalIgnoreCase))
                {
                    if (!DateTime.TryParse(filterQuery.ToLower(), out DateTime dob))
                    {
                        return new BadRequestObjectResult("DateOfBirth could not be parsed. Please provide a valid datetime.");
                    }
                    usersQuery = usersQuery.Where(user => user.DateOfBirth == dob);
                }
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    usersQuery = isAscending ? usersQuery.OrderBy(emp => emp.Name) : usersQuery.OrderByDescending(emp => emp.Name);

                if (sortBy.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    usersQuery = isAscending ? usersQuery.OrderBy(emp => emp.Email) : usersQuery.OrderByDescending(emp => emp.Email);

                if (sortBy.Equals("DateOfBirth", StringComparison.OrdinalIgnoreCase))
                {
                    usersQuery = isAscending ? usersQuery.OrderBy(emp => emp.DateOfBirth) : usersQuery.OrderByDescending(emp => emp.DateOfBirth);
                }
            }
            var getAllArgs = new GetAllArgs();
            getAllArgs.PageNumber = pageNumber;
            getAllArgs.PageSize = pageSize;

            return usersQuery.AsQueryable().Paginate(getAllArgs).ToList();
        }

        public ActionResult CreateNewUser(User user)
        {
            if (user == null)
            {
                return new BadRequestObjectResult("User cannot be empty.");
            }

            if (user.Id == Guid.Empty)
            {
                return new BadRequestObjectResult("User id cannot be empty.");
            }

            if (string.IsNullOrEmpty(user.Name))
            {
                return new BadRequestObjectResult("User Name cannot be empty.");
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                return new BadRequestObjectResult("User Email cannot be empty.");
            }

            var age = CalculateAge(user.DateOfBirth);
            if (age < 18)
            {
                return new BadRequestObjectResult("User must be at least 18 years old.");
            }

            if (_userRepository.GetUserByID(user.Id) != null)
            {
                return new BadRequestObjectResult("User already exists.");
            }
            //user.CreatedAt = DateTime.Now;
            //user.UpdatedAt = DateTime.Now;
            _userRepository.InsertUser(user);
            _userRepository.Save();
            return new CreatedAtActionResult("GetUserById", "Users", new { id = user.Id }, user);
        }

        public ActionResult UpdateUser(User updatedUser, Guid id)
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult("ID cannot be empty.");
            }

            if (id != updatedUser.Id)
                return new BadRequestObjectResult(new { Message = "User ID mismatch." });

            var age = CalculateAge(updatedUser.DateOfBirth);
            if (age < 18)
            {
                return new BadRequestObjectResult("User must be at least 18 years old.");
            }

            var user = _userRepository.GetUserByID(id);
            if (user == null)
            {
                return new NotFoundObjectResult(new { Message = $"User with ID {id} not found." });
            }

            user.DateOfBirth = updatedUser.DateOfBirth;
            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.UpdatedAt = DateTime.Now;

            _userRepository.UpdateUser(user);
            //userRepository.UpdateUserUpdatedAt(user);

            _userRepository.Save();

            return new OkObjectResult(new { Message = "User updated successfully.", User = user });
        }

        public ActionResult DeleteUser(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult("Guid cannot be empty.");
            }

            var user = _userRepository.GetUserByID(id);
            if (user == null)
            {
                return new NotFoundObjectResult(new { Message = $"User with ID {id} not found." });
            }

            _userRepository.DeleteUser(user);
            _userRepository.Save();

            return new OkObjectResult(new { Message = "User deleted successfully." });
        }

        #endregion
    }
}
