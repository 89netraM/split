using Fido2NetLib;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Split.Application.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddEncryptionService();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddJwtService();
builder
    .Services.AddAuthorizationBuilder()
    .AddPolicy(
        "AuthenticatedUser",
        policy => policy.RequireAuthenticatedUser().AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    );

builder.Services.AddCors(options =>
    options.AddPolicy(
        name: "ApiCorsPolicy",
        policy => policy.WithOrigins("http://localhost:5173").WithHeaders("Content-Type")
    )
);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Add(AuthSerializerContext.Default)
);
builder.Services.AddOptions<Fido2Configuration>().BindConfiguration("Fido2");
builder.Services.AddTransient<IFido2>(sp => new Fido2(sp.GetRequiredService<IOptions<Fido2Configuration>>().Value));

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

var apiEndpoints = app.MapGroup("/api");
apiEndpoints.RequireCors("ApiCorsPolicy");

apiEndpoints.MapAuthEndpoints();

apiEndpoints.MapGet("/secret", () => "Secret!").RequireAuthorization("AuthenticatedUser");

app.Run();
