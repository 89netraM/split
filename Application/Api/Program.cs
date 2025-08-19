using System;
using Fido2NetLib;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Split.Application.Api;
using Split.Domain.Transaction;
using Split.Domain.User;
using Split.Infrastructure.Encryptor;
using Split.Infrastructure.PhoneNumberVerifier;
using Split.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddRepositories();

builder.Services.AddEncryptionService().AddPhoneNumberVerifierServices();

builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

builder.Services.AddSingleton(TimeProvider.System).AddSingleton(Random.Shared);

builder.Services.AddTransient<UserService>().AddTransient<TransactionService>().AddTransient<AuthService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddJwtService();
builder
    .Services.AddAuthorizationBuilder()
    .AddPolicy(
        "AuthenticatedUser",
        policy => policy.RequireAuthenticatedUser().AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    );
builder
    .Services.AddHttpContextAccessor()
    .AddScoped(sp =>
        sp.GetRequiredService<IHttpContextAccessor>().HttpContext
        ?? throw new ArgumentNullException(nameof(IHttpContextAccessor.HttpContext))
    );

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Add(AuthSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Add(UserSerializerContext.Default);
    options.SerializerOptions.TypeInfoResolverChain.Add(TransactionSerializerContext.Default);
});
builder.Services.AddOptions<Fido2Configuration>().BindConfiguration("Fido2");
builder.Services.AddTransient<IFido2>(sp => new Fido2(sp.GetRequiredService<IOptions<Fido2Configuration>>().Value));

var app = builder.Build();

app.MapOpenApi();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

var apiEndpoints = app.MapGroup("/api");

apiEndpoints.MapAuthEndpoints();
apiEndpoints.MapUsersEndpoints().RequireAuthorization("AuthenticatedUser");
apiEndpoints.MapTransactionsEndpoints().RequireAuthorization("AuthenticatedUser");

app.Run();
