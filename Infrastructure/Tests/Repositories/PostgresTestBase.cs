using System.Collections.Generic;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Infrastructure.Repositories;

namespace Split.Infrastructure.Tests.Repositories;

public abstract class PostgresTestBase
{
#nullable disable
    protected ServiceProvider Services { get; private set; }
    protected IMediator Mediator { get; private set; }

#nullable restore

    [TestInitialize]
    public async Task BeforeEach()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton(
                new ConfigurationBuilder()
                    .AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:database"] = Containers.Postgres.GetConnectionString(),
                            ["Logging:LogLevel:Default"] = "Debug",
                        }
                    )
                    .Build()
            )
            .AddSingleton<IConfiguration>(sp => sp.GetRequiredService<IConfigurationRoot>());
        services.AddLogging();
        services.AddRepositories();
        services.AddSingleton(Mediator = Substitute.For<IMediator>());
        Services = services.BuildServiceProvider();

        var database = Services.GetRequiredService<SplitDbContext>().Database;
        await database.EnsureDeletedAsync();
        await database.EnsureCreatedAsync();
    }

    [TestCleanup]
    public async Task AfterEach()
    {
        await Services.DisposeAsync();
    }
}
