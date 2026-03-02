using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementAPI.Database;

namespace TestProject1
{
    public class MockUserRepository : IUserRepository, IDisposable
    {
        private UserContext context;

        private static List<UserManagementAPI.Model.User> GetFakeUserList()
        {
            return new List<UserManagementAPI.Model.User>()
            {
                new UserManagementAPI.Model.User
                {
                    Id = new Guid(),
                    Name = "John Doe",
                    Email = "J.D@gmail.com",
                    CreatedAt = DateTime.Now,
                    DateOfBirth = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new UserManagementAPI.Model.User
                {
                    Id = new Guid(),
                    Name = "Mark Luther",
                    Email = "M.L@gmail.com",
                    CreatedAt = DateTime.Now,
                    DateOfBirth = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };
        }

        public MockUserRepository(UserContext context)
        {
            this.context = context;
        }

        public SqliteConnection GetConnection()
        {
            return (SqliteConnection)context.Database.GetDbConnection();
        }

        public void DeleteUser(UserManagementAPI.Model.User user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUserAsync(UserManagementAPI.Model.User id)
        {
            throw new NotImplementedException();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<IEnumerable<UserManagementAPI.Model.User>> GetCompleteUsersAsync()
        {
            var users = GetFakeUserList();
            return users;
            //throw new NotImplementedException();
        }

        public UserManagementAPI.Model.User GetUserByID(Guid userId)
        {
            return null;
            //throw new NotImplementedException();
        }

        public Task<UserManagementAPI.Model.User> GetUserByIdAsync(Guid userId)
        {
            return null;
        }

        public IEnumerable<UserManagementAPI.Model.User> GetUsers()
        {
            throw new NotImplementedException();
        }

        public void InsertUser(UserManagementAPI.Model.User user)
        {
            throw new NotImplementedException();
        }

        public Task InsertUserAsync(UserManagementAPI.Model.User user)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync()
        {
            throw new NotImplementedException();
        }

        public void UpdateUser(UserManagementAPI.Model.User user)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserAsync(UserManagementAPI.Model.User user)
        {
            throw new NotImplementedException();
        }
    }
}
