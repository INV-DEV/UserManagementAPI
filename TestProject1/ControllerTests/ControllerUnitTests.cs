using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserManagementAPI.Controllers;
using UserManagementAPI.Model;

namespace TestProject1.ControllerTests
{
    [TestClass]
    public class ControllerUnitTests
    {
        [TestMethod]
        public async Task TestUsersController_GetAllUsers_OkObjectResult_Empty()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockusersService = new MockUsersService();
            var controller = new UsersController(mockLogger.Object, mockusersService);

            string? filterOn = null;
            string? filterQuery = null;
            string? sortBy = null;
            bool isAscending = true;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var actionResult = await controller.GetAllUsers(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsNull(actionResult.Value);
            var userModelList = ((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult.Result).Value as IEnumerable<UserModel>;
            Assert.IsNotNull(userModelList);
            Assert.IsInstanceOfType(userModelList, typeof(IEnumerable<UserModel>));
            Assert.IsTrue(userModelList.Count() == 0);
        }

        [TestMethod]
        public async Task TestUsersController_GetAllUsers_OkObjectResult_Count2()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockusersService = new MockUsersService();
            mockusersService._users = GetFakeUserList();
            var controller = new UsersController(mockLogger.Object, mockusersService);

            string? filterOn = null;
            string? filterQuery = null;
            string? sortBy = null;
            bool isAscending = true;
            int pageNumber = 1;
            int pageSize = 10;

            // Act
            var actionResult = await controller.GetAllUsers(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsNull(actionResult.Value);
            var userModelList = ((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult.Result).Value as IEnumerable<UserModel>;
            Assert.IsNotNull(userModelList);
            Assert.IsInstanceOfType(userModelList, typeof(IEnumerable<UserModel>));
            Assert.IsTrue(userModelList.Count() == 2);
        }

        [TestMethod]
        public async Task TestUsersController_GetUserById_NotFoundObjectResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockUsersService = new MockUsersService();
            var controller = new UsersController(mockLogger.Object, mockUsersService);

            var id = "6e25ad6c-58ab-4840-82a2-08800c4e403d";
            var idGuid = new Guid(id);
            var message = @"{ Message = User with ID 6e25ad6c-58ab-4840-82a2-08800c4e403d not found. }";

            // Act
            var actionResult = await controller.GetUserById(idGuid);

            // Assert
            Assert.IsNull(actionResult.Value);
            Assert.IsInstanceOfType(actionResult.Result, typeof(NotFoundObjectResult));
            Assert.IsTrue(((NotFoundObjectResult)actionResult.Result).StatusCode == 404);
            NotFoundObjectResult? notFoundObjectResult = actionResult.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundObjectResult);
            Assert.IsNotNull(notFoundObjectResult.Value);
            Assert.AreEqual(notFoundObjectResult.Value.ToString(), message);
        }

        [TestMethod]
        public async Task TestUsersController_GetUserById_OkObjectResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockUsersService = new MockUsersService();
            var id = "4e25ad6c-58ab-4840-82a2-08800c4e43c3";
            var idGuid = new Guid(id);
            mockUsersService._user = GetFakeUser(idGuid);
            var controller = new UsersController(mockLogger.Object, mockUsersService);

            // Act
            var actionResult = await controller.GetUserById(idGuid);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsNotNull(actionResult.Result);
            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));
            Assert.IsTrue(((OkObjectResult)actionResult.Result).StatusCode == 200);
            OkObjectResult? okObjectResult = actionResult.Result as OkObjectResult;
            Assert.IsNotNull(okObjectResult);
            var userModel = ((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult.Result).Value as UserModel;
            Assert.IsNotNull(userModel);
            Assert.IsInstanceOfType(userModel, typeof(UserModel));
        }

        [TestMethod]
        public async Task TestUsersController_Post_CreatedAtActionResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockUsersService = new MockUsersService();
            var userModel = GetFakeUserModel();
            var controller = new UsersController(mockLogger.Object, mockUsersService);
            
            // Act
            var actionResult = await controller.Post(userModel);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(CreatedAtActionResult));
            Assert.IsTrue(((CreatedAtActionResult)actionResult).StatusCode == 201);
            Assert.AreEqual(userModel.Id, ((UserManagementAPI.Model.UserModel)((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value).Id);
        }

        [TestMethod]
        public async Task TestUsersController_Post_BadRequestObjectResult_UserUnder18()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockUsersService = new MockUsersService();
            var userModel = GetFakeUserModel(DateTime.Now.AddYears(-17));
            var controller = new UsersController(mockLogger.Object, mockUsersService);

            // Act
            var actionResult = await controller.Post(userModel);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
            Assert.IsTrue(((BadRequestObjectResult)actionResult).StatusCode == 400);
            Assert.IsNotNull(((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value);
            var message = ((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value.ToString();
            Assert.AreEqual("{ Message = User must be at least 18 years old. }", message);
        }

        [TestMethod]
        public async Task TestUsersController_Put_BadRequestObjectResult_UserUnder18()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockUsersService = new MockUsersService();
            var userModel = GetFakeUserModel(DateTime.Now.AddYears(-17));
            var controller = new UsersController(mockLogger.Object, mockUsersService);

            // Act
            var actionResult = await controller.Put(userModel, userModel.Id);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
            Assert.IsTrue(((BadRequestObjectResult)actionResult).StatusCode == 400);
            Assert.IsNotNull(((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value);
            var message = ((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value.ToString();
            Assert.AreEqual("{ Message = User must be at least 18 years old. }", message);
        }

        [TestMethod]
        public async Task TestUsersController_Put_NotFoundObjectResult_UserNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockUsersService = new MockUsersService();
            var userModel = GetFakeUserModel();
            var controller = new UsersController(mockLogger.Object, mockUsersService);

            // Act
            var actionResult = await controller.Put(userModel, userModel.Id);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(NotFoundObjectResult));
            Assert.IsTrue(((NotFoundObjectResult)actionResult).StatusCode == 404);
            Assert.IsNotNull(((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value);
            var message = ((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value.ToString();
            Assert.IsTrue(message.Contains(userModel.Id.ToString()));
            var messageExpected = "{ Message = User with ID " + userModel.Id + " not found. }";
            Assert.AreEqual(messageExpected, message);
        }

        [TestMethod]
        public async Task TestUsersController_Put_OkObjectResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockUsersService = new MockUsersService();
            var userModel = GetFakeUserModel();
            mockUsersService._user = GetFakeUser(userModel.Id);
            var controller = new UsersController(mockLogger.Object, mockUsersService);

            // Act
            var actionResult = await controller.Put(userModel, userModel.Id);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            Assert.IsTrue(((OkObjectResult)actionResult).StatusCode == 200);
            Assert.IsNotNull(((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value);
            //var message = ((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value.ToString();
            //Assert.IsTrue(message.Contains(userModel.Id.ToString()));
            //var messageExpected = "{ Message = User with ID " + userModel.Id + " not found. }";
            //new { Message = "User updated successfully.", UserModel = GetUserModel(user) }
            //Assert.AreEqual(messageExpected, message);
        }

        [TestMethod]
        public async Task TestUsersController_Delete_NotFoundObjectResult_UserNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockUsersService = new MockUsersService();
            var userModel = GetFakeUserModel();
            var controller = new UsersController(mockLogger.Object, mockUsersService);

            // Act
            var actionResult = await controller.Delete(userModel.Id);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(NotFoundObjectResult));
            Assert.IsTrue(((NotFoundObjectResult)actionResult).StatusCode == 404);
            Assert.IsNotNull(((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value);
            var message = ((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value.ToString();
            Assert.IsTrue(message.Contains(userModel.Id.ToString()));
            var messageExpected = "{ Message = User with ID " + userModel.Id + " not found. }";
            Assert.AreEqual(messageExpected, message);
        }

        [TestMethod]
        public async Task TestUsersController_Delete_OkObjectResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UsersController>>();
            var mockUsersService = new MockUsersService();
            var userModel = GetFakeUserModel();
            mockUsersService._user = GetFakeUser(userModel.Id);
            var controller = new UsersController(mockLogger.Object, mockUsersService);

            // Act
            var actionResult = await controller.Delete(userModel.Id);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            Assert.IsTrue(((OkObjectResult)actionResult).StatusCode == 200);
            Assert.IsNotNull(((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value);
            var message = ((Microsoft.AspNetCore.Mvc.ObjectResult)actionResult).Value.ToString();
            Assert.AreEqual("{ Message = User deleted successfully. }", message);
        }

        private static UserManagementAPI.Model.User GetFakeUser(Guid id)
        {
            return new UserManagementAPI.Model.User()
            {
                Id = id,
                Name = "John Doe",
                Email = "J.D@gmail.com",
                DateOfBirth = DateTime.Now.AddYears(-19),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        private static UserManagementAPI.Model.User GetFakeUser()
        {
            return new UserManagementAPI.Model.User()
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "J.D@gmail.com",
                DateOfBirth = DateTime.Now.AddYears(-19),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        private static UserManagementAPI.Model.UserModel GetFakeUserModel()
        {
            return new UserManagementAPI.Model.UserModel()
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "J.D@gmail.com",
                DateOfBirth = DateTime.Now.AddYears(-19),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        private static UserManagementAPI.Model.UserModel GetFakeUserModel(Guid id)
        {
            return new UserManagementAPI.Model.UserModel()
            {
                Id = id,
                Name = "John Doe",
                Email = "J.D@gmail.com",
                DateOfBirth = DateTime.Now.AddYears(-19),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        private static UserManagementAPI.Model.UserModel GetFakeUserModel(DateTime dateofbirth)
        {
            return new UserManagementAPI.Model.UserModel()
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "J.D@gmail.com",
                DateOfBirth = dateofbirth,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        private static List<UserManagementAPI.Model.User> GetFakeUserList()
        {
            return new List<UserManagementAPI.Model.User>()
            {
                new UserManagementAPI.Model.User
                {
                    Id = new Guid(),
                    Name = "John Doe",
                    Email = "J.D@gmail.com",
                    DateOfBirth = DateTime.Now.AddYears(-30),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new UserManagementAPI.Model.User
                {
                    Id = new Guid(),
                    Name = "Mark Luther",
                    Email = "M.L@gmail.com",
                    DateOfBirth = DateTime.Now.AddYears(-40),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };
        }
    }
}
