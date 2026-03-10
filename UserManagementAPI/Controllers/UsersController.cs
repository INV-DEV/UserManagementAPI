using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using UserManagementAPI.Database;
using UserManagementAPI.DTOs;
using UserManagementAPI.Model;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUsersService _usersService;
        private UserContext _context;

        public UsersController(ILogger<UsersController> logger, IUsersService context)
        {
            _logger = logger;
            _usersService = context;
            _context = context.GetUserContext();
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="registerDto"></param>
        /// <returns></returns>
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            // Validate the incoming model.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if the email already exists.
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());

            if (existingUser != null)
            {
                return Conflict(new { message = "Email is already registered." });
            }

            // Hash the password using BCrypt.
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create a new user entity.
            var newUser = new User
            {
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = hashedPassword
            };
            // Add the new user to the database.
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            // Optionally, assign a default role to the new user.
            // For example, assign the "User" role.
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole != null)
            {
                                var newUserRole = new UserRole
                                {
                                    UserId = newUser.Id,
                                    RoleId = userRole.Id
                                };
                _context.UserRoles.Add(newUserRole);
                await _context.SaveChangesAsync();
            }
            return CreatedAtAction(nameof(GetProfile), new { id = newUser.Id }, new { message = "User registered successfully." });
        }

        /// <summary>
        /// Retrieves the authenticated user's profile.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetProfile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            // Extract the user's email from the JWT token claims.
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);
            if (emailClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: Email claim missing." });
            }
            string userEmail = emailClaim.Value;
            // Retrieve the user from the database, including roles.
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail.ToLower());
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            // Map the user entity to ProfileDTO.
            var profile = new ProfileDTO
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };

            return Ok(profile);
        }

        /// <summary>
        /// Updates the authenticated user's profile.
        /// </summary>
        /// <param name="updateDto"></param>
        /// <returns></returns>
        [HttpPut("UpdateProfile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO updateDto)
        {
            // Validate the incoming model.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Extract the user's email from the JWT token claims.
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);
            if (emailClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: Email claim missing." });
            }
            string userEmail = emailClaim.Value;
            // Retrieve the user from the database.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail.ToLower());
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Update fields if provided.
            if (!string.IsNullOrEmpty(updateDto.Name))
            {
                user.Name = updateDto.Name;
            }

            if (!string.IsNullOrEmpty(updateDto.Email))
            {
                // Check if the new email is already taken by another user.
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == updateDto.Email.ToLower() && u.Id != user.Id);
                if (emailExists)
                {
                    return Conflict(new { message = "Email is already in use by another account." });
                }
                user.Email = updateDto.Email;
            }
            if (!string.IsNullOrEmpty(updateDto.Password))
            {
                // Hash the new password before storing.
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(updateDto.Password);
                user.Password = hashedPassword;
            }
            // Save the changes to the database.
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully." });
        }

        /// <summary>
        /// Get user by Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize()]
        public async Task<ActionResult<UserModel>> GetUserById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return new BadRequestObjectResult(new { Message = $"ID cannot be empty." });

            try
            {
                var user = await _usersService.GetUserByIdAsync(id);

                if (user == null)
                    return new NotFoundObjectResult(new { Message = $"User with ID {id} not found." });

                return new OkObjectResult(GetUserModel(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving the user.");
                return StatusCode(500, "Error occurred while retrieving the user.");
            }
        }

        /// <summary>
        /// Get All users. Allows sorting and filtering on GET /users (by name, email, or age).
        /// </summary>
        /// <param name="filterOn"></param>
        /// <param name="filterQuery"></param>
        /// <param name="sortBy"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserModel>?>?> GetAllUsers(
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                IEnumerable<User?> response;

                if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
                {
                    if (filterOn.Equals("DateOfBirth", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!DateTime.TryParse(filterQuery.ToLower(), out DateTime dob))
                        {
                            return new BadRequestObjectResult("DateOfBirth could not be parsed. Please provide a valid datetime.");
                        }
                        response = await _usersService.GetAllUsersAsync(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize, dob);
                        return new OkObjectResult(response);
                    }
                }

                response = await _usersService.GetAllUsersAsync(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize, null);

                return new OkObjectResult(GetUsersModel(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users.");
                return StatusCode(500, "Error occurred while retrieving all users.");
            }
        }

        /// <summary>
        /// Create a user. Users must be at least 18 years old. Emails must be unique. Will return BadRequest response if the email is already in use.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost()]
        [Authorize]
        public async Task<ActionResult> Post([FromBody] UserModel user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (user == null)
                    return new BadRequestObjectResult(new { Message = "User cannot be empty." });

                if (user.Id == Guid.Empty)
                    return new BadRequestObjectResult(new { Message = "User ID cannot be empty." });

                if (string.IsNullOrEmpty(user.Name))
                    return new BadRequestObjectResult(new { Message = "User Name cannot be empty." });

                if (string.IsNullOrEmpty(user.Email))
                    return new BadRequestObjectResult(new { Message = "User Email cannot be empty." });

                var age = CalculateAge(user.DateOfBirth);
                if (age < 18)
                    return new BadRequestObjectResult(new { Message = "User must be at least 18 years old." });

                if (await _usersService.GetUserByIdAsync(user.Id) != null)
                    return new BadRequestObjectResult(new { Message = "User already exists." });
                
                var result = await _usersService.CreateNewUserAsync(GetUser(user));

                return new CreatedAtActionResult(nameof(GetUserById), "Users", new { id = user.Id }, GetUserModel(result));
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                _logger.LogError(ex, "Post: Microsoft.EntityFrameworkCore.DbUpdateException");
                if (ex.InnerException != null && ex.InnerException.Message.Contains("UNIQUE constraint failed: Users.Email")) //SqliteException
                    return BadRequest("User with the email address already in use.");

                _logger.LogError(ex, "Error occurred while creating a user.");
                return StatusCode(500, "Error occurred while creating a user.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a user.");
                return StatusCode(500, "Error occurred while creating a user.");
            }
        }

        /// <summary>
        /// Update user details
        /// </summary>
        /// <param name="updatedUser"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Put([FromBody] UserModel updatedUser, [FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (id == Guid.Empty)
                    return new BadRequestObjectResult(new { Message = "ID cannot be empty." });

                if (id != updatedUser.Id)
                    return new BadRequestObjectResult(new { Message = "User ID mismatch." });

                var age = CalculateAge(updatedUser.DateOfBirth);
                if (age < 18)
                    return new BadRequestObjectResult(new { Message = "User must be at least 18 years old." });

                var user = await _usersService.GetUserByIdAsync(id);
                if (user == null)
                    return new NotFoundObjectResult(new { Message = $"User with ID {id} not found." });

                await _usersService.UpdateUserAsync(GetUser(updatedUser), user);
                return new OkObjectResult(new { Message = "User updated successfully.", UserModel = GetUserModel(user) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating a user.");
                return StatusCode(500, "Error occurred while updating a user.");
            }
        }

        /// <summary>
        /// Delete a user 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return new BadRequestObjectResult(new { Message = "Guid cannot be empty." });

            try
            {
                var user = await _usersService.GetUserByIdAsync(id);
                if (user == null)
                    return new NotFoundObjectResult(new { Message = $"User with ID {id} not found." });

                await _usersService.DeleteUserAsync(user);

                return new OkObjectResult(new { Message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting a user.");
                return StatusCode(500, "Error occurred while deleting a user.");
            }
        }

        private static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age)) age--;

            return age;
        }

        private List<UserModel> GetUsersModel(IEnumerable<User?> users)
        {
            var usersModelList = new List<UserModel>();
            foreach (var user in users)
                if(user != null)
                {
                    usersModelList.Add(new()
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        DateOfBirth = user.DateOfBirth,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    });
                }
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

        private User GetUser(UserModel user)
            =>
            new User()
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
    }
}

