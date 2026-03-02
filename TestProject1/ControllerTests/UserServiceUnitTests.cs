using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementAPI.Controllers;
using UserManagementAPI.Database;
using UserManagementAPI.Services;

namespace TestProject1.ControllerTests
{
    [TestClass]
    public class UserServiceUnitTests
    {
        [TestMethod]
        public async Task TestGetUserById()
        {
            //Arrange

            //User user = new User();
            //Logger<UsersController> logger = new Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger<UsersController>();
            Guid id = Guid.NewGuid();

            UsersService _usersService = new UsersService(null, 
                new UserContext(new Microsoft.EntityFrameworkCore.DbContextOptions<UserContext>()));

            //Act
            var result = await _usersService.GetUserByIdAsync(id);

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task TestGetAllUsersPaginated()
        {
            //Arrange
            Guid id = Guid.NewGuid();

            string? filterOn = null;
            string? filterQuery = null;
            string? sortBy = null;
            bool isAscending = true;
            int pageNumber = 1;
            int pageSize = 10;
            UsersService _usersService = new UsersService(null, null);

            //Act
            var result = await _usersService.GetAllUsersAsync(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task TestUsersController_GetUserById_NotFoundResult()
        {
            // Arrange
            var mockRepository = new Mock<IUserRepository>();
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockUserContext = new Mock<IUsersService>();
            var context = new UserContext(new Microsoft.EntityFrameworkCore.DbContextOptions<UserContext>());

            var controller = new UsersController(mockLogger.Object, new UsersService(mockLogger.Object, context));

            var id = new Guid("6e25ad6c-58ab-4840-82a2-08800c4e403d");
            // Act
            var actionResult = await controller.GetUserById(id);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(NotFoundObjectResult));
        }
    }
}
