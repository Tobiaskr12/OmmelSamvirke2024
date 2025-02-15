using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccess.Base;

[UsedImplicitly]
public class OmmelSamvirkeDesignTimeDbContextFactory : IDesignTimeDbContextFactory<OmmelSamvirkeDbContext>
{
    public OmmelSamvirkeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OmmelSamvirkeDbContext>();
        optionsBuilder.UseSqlServer();

        return new OmmelSamvirkeDbContext(optionsBuilder.Options);
    }
}
