using Microsoft.AspNetCore.Mvc;
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

        public UsersController(ILogger<UsersController> logger, IUsersService context)
        {
            _logger = logger;
            _usersService = context;
        }

        /// <summary>
        /// Get user by Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById([FromRoute] Guid id)
        {
            try
            {
                return await _usersService.GetUserByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving the user.");
                return StatusCode(500);
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
        public async Task<ActionResult<IEnumerable<GetAllUsersResponse>>> GetAllUsers(
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var result = await _usersService.GetAllUsersAsync(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users.");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Create a user. Users must be at least 18 years old. Emails must be unique. Will return BadRequest response if the email is already in use.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost()]
        public async Task<ActionResult> Post([FromBody] User user)
        {
            try
            {
                return await _usersService.CreateNewUserAsync(user);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                _logger.LogError(ex, "Post: Microsoft.EntityFrameworkCore.DbUpdateException");
                if (ex.InnerException != null && ex.InnerException.Message.Contains("UNIQUE constraint failed: Users.Email")) //SqliteException
                {
                    return BadRequest("User with the email address already in use.");
                }

                _logger.LogError(ex, "Error occurred while creating a user.");
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a user.");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Update user details
        /// </summary>
        /// <param name="updatedUser"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<ActionResult> Put([FromBody] User updatedUser, [FromRoute] Guid id)
        {
            try
            {
                return await _usersService.UpdateUserAsync(updatedUser, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating a user.");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Delete a user 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                return await _usersService.DeleteUserAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting a user.");
                return StatusCode(500);
            }
        }
    }
}

