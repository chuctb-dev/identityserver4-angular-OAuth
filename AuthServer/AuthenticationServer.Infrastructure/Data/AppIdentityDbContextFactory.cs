using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthenticationServer.Infrastructure.Data
{
    public class AppIdentityDbContextFactory : IDesignTimeDbContextFactory<AppIdentityDbContext>
    {
        public AppIdentityDbContext CreateDbContext(string[] args)
        {
            var buider = new DbContextOptionsBuilder<AppIdentityDbContext>();
            buider.UseSqlServer("Server=localhost;Database=sample_identity_db;User Id=sa;Password=604708;Encrypt=False;");
            return new AppIdentityDbContext(buider.Options);
        }
    }
}
