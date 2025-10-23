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
        public static void SeedDefaultGL(SqliteConnectionFactory factory)
        {
            var repo = new GroupLeaderRepository(factory);
            var admin = repo.GetByLogin("admin");
            if (admin is null)
            {
                repo.Add(new GroupLeader
                {
                    Name = "Administrador",
                    Login = "admin",
                    AccessLevel = AccessLevel.Admin
                }, plainPassword: "admin");
            }
        }
    }
}
