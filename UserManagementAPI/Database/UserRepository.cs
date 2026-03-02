using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Runtime.InteropServices;
using UserManagementAPI.Model;

namespace UserManagementAPI.Database
{
    public class UserRepository : IUserRepository, IDisposable
    {
        private UserContext context;
        
        public UserRepository(UserContext context)
        {
            this.context = context;
        }

        public SqliteConnection GetConnection()
        {
            return (SqliteConnection) context.Database.GetDbConnection();
        }

        public async Task<IEnumerable<User>> GetCompleteUsersAsync()
        {
            var result = await context.Users.ToListAsync();
            return result;
        }

        public async Task DeleteUserAsync(User id)
        {
            context.Users.Remove(id);
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            return await context.Users.FindAsync(userId);
        }

        public async Task UpdateUserAsync(User user)
        {
            context.Entry(user).State = EntityState.Modified;
        }

        public async Task InsertUserAsync(User user)
        {
            await context.Users.AddAsync(user);
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }

        public IEnumerable<User> GetUsers()
        {
            return context.Users.ToList();
        }

        public User GetUserByID(Guid id)
        {
            return context.Users.Find(id);
        }

        public void InsertUser(User user)
        {
            context.Users.Add(user);
        }

        public void UpdateUserUpdatedAt(User user)
        {
            string sql = "UPDATE Users SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = @Id;";
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteUser(User user)
        {
            context.Users.Remove(user);
        }

        public void UpdateUser(User user)
        {
            context.Entry(user).State = EntityState.Modified;
        }

        public void Save()
        {
            context.SaveChanges();
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
    }
}
