using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testcontainers.PostgreSql;

namespace Split.Infrastructure.Tests.Repositories;

[TestClass]
public static class Containers
{
#nullable disable
    public static PostgreSqlContainer Postgres { get; private set; }

#nullable restore

    [AssemblyInitialize]
    public static async Task Initialize(TestContext _)
    {
        Postgres = new PostgreSqlBuilder().WithDatabase("database").Build();
        await Postgres.StartAsync();
    }

    [AssemblyCleanup()]
    public static async Task Cleanup()
    {
        await Postgres.DisposeAsync();
    }
}
