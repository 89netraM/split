using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Split.Application.Api.Discord;

public sealed class DiscordService : IHostedService, IAsyncDisposable
{
    private readonly ILogger<DiscordService> logger;
    private readonly ILogger<DiscordSocketClient> clientLogger;
    private readonly IServiceProvider serviceProvider;
    private readonly string discordToken;
    private readonly DiscordSocketClient discordClient;
    private readonly InteractionService interactionService;

    public DiscordService(
        ILogger<DiscordService> logger,
        ILogger<DiscordSocketClient> clientLogger,
        IServiceProvider serviceProvider,
        IOptions<DiscordOptions> options
    )
    {
        this.logger = logger;
        this.clientLogger = clientLogger;
        this.serviceProvider = serviceProvider;
        discordToken = options.Value.Token;

        discordClient = new(new() { GatewayIntents = GatewayIntents.None, UseInteractionSnowflakeDate = false });
        interactionService = new(discordClient);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        discordClient.Ready += OnReady;
        discordClient.Log += OnLog;
        discordClient.SlashCommandExecuted += OnSlashCommandExecuted;

        interactionService.Log += OnLog;

        await discordClient.LoginAsync(TokenType.Bot, discordToken);
        await discordClient.StartAsync();
    }

    private async Task OnReady()
    {
        await interactionService.AddModuleAsync<CreateTransactionCommand>(serviceProvider);
        await interactionService.AddModuleAsync<ViewBalancesCommand>(serviceProvider);
        await interactionService.RegisterCommandsGloballyAsync(deleteMissing: true);
        discordClient.Ready -= OnReady;
    }

    private async Task OnLog(LogMessage logMessage)
    {
        var logLevel = logMessage.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.Information,
        };

#pragma warning disable CA2254 // Template should be a static expression, but can't in the case of forwarding messages from the Discord package
        clientLogger.Log(logLevel, logMessage.Exception, logMessage.Message);
#pragma warning restore CA2254
    }

    private async Task OnSlashCommandExecuted(SocketSlashCommand command)
    {
        await interactionService.ExecuteCommandAsync(
            new SocketInteractionContext<SocketSlashCommand>(discordClient, command),
            serviceProvider
        );
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        discordClient.Ready -= OnReady;
        discordClient.Log -= OnLog;
        discordClient.SlashCommandExecuted -= OnSlashCommandExecuted;

        interactionService.Log -= OnLog;

        await discordClient.StopAsync();
    }

    public async ValueTask DisposeAsync()
    {
        discordClient.Ready -= OnReady;
        discordClient.Log -= OnLog;
        discordClient.SlashCommandExecuted -= OnSlashCommandExecuted;

        interactionService.Log -= OnLog;
        interactionService.Dispose();

        await discordClient.DisposeAsync();
    }
}
