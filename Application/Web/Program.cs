using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Split.Application.Web.Auth;
using Split.Application.Web.Components;
using Split.Domain.Transaction;
using Split.Domain.User;
using Split.Infrastructure.Repositories;
using Split.Infrastructure.Swish;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRepositories();

builder.Services.AddGitHubAuthentication().AddIsUserAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddLocalization();

builder.Services.AddSingleton(TimeProvider.System).AddScoped<UserService>().AddScoped<TransactionService>();

builder.Services.AddSingleton<SwishService>();

builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication().UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.UseRequestLocalization(
    new RequestLocalizationOptions()
        .AddSupportedCultures(["en", "en-US", "en-GB", "sv", "sv-SE"])
        .SetDefaultCulture("sv-SE")
);

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
