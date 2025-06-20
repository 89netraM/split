using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Split.Domain.Transaction;
using Split.Domain.User;

namespace Split.Infrastructure.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRelationshipRepository, UserRelationshipRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddDbContext<SplitDbContext>(
            (sp, options) => options.UseNpgsql(sp.GetRequiredService<IConfiguration>().GetConnectionString("database"))
        );
        services.AddHostedService<StartupMigration>();
        return services;
    }
}
