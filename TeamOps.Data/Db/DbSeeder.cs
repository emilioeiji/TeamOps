// Project: TeamOps.Data
// File: Db/DbSeeder.cs
using TeamOps.Data.Db;
using TeamOps.Data.Repositories;
using TeamOps.Core.Entities;
using TeamOps.Core.Common;

namespace TeamOps.Data
{
    public static class DbSeeder
    {
        public static void SeedDefaultAdmin(SqliteConnectionFactory factory)
        {
            var repo = new UserRepository(factory);
            var admin = repo.GetByLogin("admin");
            if (admin is null)
            {
                repo.Add(new User
                {
                    Name = "Administrador",
                    Login = "admin",
                    AccessLevel = AccessLevel.Admin,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                    CreatedAt = DateTime.Now
                });
            }
        }
    }
}