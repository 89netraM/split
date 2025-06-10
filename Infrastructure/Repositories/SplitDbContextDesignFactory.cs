using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Split.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public class SplitDbContextDesignFactory : IDesignTimeDbContextFactory<SplitDbContext>
{
    public SplitDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SplitDbContext>();
        optionsBuilder.UseNpgsql();

        return new SplitDbContext(optionsBuilder.Options);
    }
}
