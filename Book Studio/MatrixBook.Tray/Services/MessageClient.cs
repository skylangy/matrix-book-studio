using MatrixBook.Tray.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MatrixBook.Tray.Services;
public class MessageClient : IMessageClient
{
    private readonly Configuration _configuration;
    private readonly ILogger<MessageClient> _logger;
    private readonly List<Action<CommandModel>> _executeCommandHandlers = [];
    private readonly IMessageMediator _messageMediator;
    private const int MaxRetryAttempts = 10;
    private const int DelayBetweenRetriesInSeconds = 15;
    private const int ReconnectDelayInSeconds = 30;

    private readonly Dictionary<string, HubConnection> _hubConnections = [];
    private bool _initialized = false;

    public MessageClient(
        IOptions<Configuration> configuration,
        IMessageMediator messageMediator,
        ILogger<MessageClient> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        _configuration = configuration.Value;
        _messageMediator = messageMediator;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        Initialize();

        foreach (var (serverName, connection) in _hubConnections)
        {
            if (connection.State == HubConnectionState.Connected || connection.State == HubConnectionState.Connecting)
            {
                _logger.LogInformation("Connection to {ServerName} already in state {State}. Skipping.", serverName, connection.State);
                continue;
            }

            int retryAttempts = 0;
            while (retryAttempts < MaxRetryAttempts)
            {
                try
                {
                    _logger.LogInformation("Starting connection to {ServerName}", serverName);
                    await connection.StartAsync();
                    _logger.LogInformation("Connected to {ServerName}", serverName);
                    _messageMediator.Send(new ConnectionMessage(serverName, true));
                    break;
                }
                catch (Exception ex)
                {
                    retryAttempts++;
                    _logger.LogError("Failed to connect to {ServerName}: {Message}. Attempt {Retry}/{Max}", serverName, ex.Message, retryAttempts, MaxRetryAttempts);
                    await Task.Delay(TimeSpan.FromSeconds(DelayBetweenRetriesInSeconds));
                }
            }
        }
    }

    public async Task SendMessageAsync(string message)
    {
        foreach (var hub in _hubConnections.Values)
        {
            if (hub.State != HubConnectionState.Connected)
            {
                _logger.LogWarning("HubConnection is not connected. Cannot send message.");
                continue;
            }

            await hub.InvokeAsync("SendMessage", message);
        }
    }

    public void AddExecuteCommandHandler(Action<CommandModel> handler)
    {
        if (handler != null)
        {
            _executeCommandHandlers.Add(handler);
        }
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        foreach (var server in _configuration.Servers)
        {
            if (!server.Enabled)
                continue;

            if (_hubConnections.TryGetValue(server.Name, out var existingConnection))
            {
                if (existingConnection.State != HubConnectionState.Disconnected)
                {
                    _logger.LogInformation("Existing connection for {ServerName} is active. Skipping init.", server.Name);
                    continue;
                }
            }

            _logger.LogInformation("Initializing message client with {serverUrl}", server.Url);

            var hubConnection = new HubConnectionBuilder()
                .WithUrl(server.Url)
                .WithAutomaticReconnect()
                .Build();

            hubConnection.On<string>("ExecuteCommand", message =>
            {
                _logger.LogInformation("Message from server: {message}", message);
                if (!string.IsNullOrEmpty(message))
                {
                    var command = JsonSerializer.Deserialize<CommandModel>(message);
                    if (command != null)
                    {
                        foreach (var handler in _executeCommandHandlers)
                        {
                            handler(command);
                        }
                    }
                }
            });

            hubConnection.Reconnected += connectionId =>
            {
                _logger.LogInformation("Reconnected to {ServerName} with connection ID: {ConnectionId}", server.Name, connectionId);
                _messageMediator.Send(new ConnectionMessage(server.Name, true));
                return Task.CompletedTask;
            };

            hubConnection.Reconnecting += exception =>
            {
                _logger.LogInformation("Reconnecting to {ServerName} due to: {Exception}", server.Name, exception?.Message);
                _messageMediator.Send(new ConnectionMessage(server.Name, false));
                return Task.CompletedTask;
            };

            hubConnection.Closed += async exception =>
            {
                _logger.LogInformation("Connection to {ServerName} closed: {Exception}", server.Name, exception?.Message);
                _messageMediator.Send(new ConnectionMessage(server.Name, false));
                await ReconnectAsync(hubConnection, server.Name);
            };

            _hubConnections[server.Name] = hubConnection;
        }

        _initialized = true;
    }

    private async Task ReconnectAsync(HubConnection hubConnection, string serverName)
    {
        int retryAttempts = 0;
        while (retryAttempts < MaxRetryAttempts)
        {
            try
            {
                await hubConnection.StartAsync();
                _logger.LogInformation("Reconnected to {ServerName}.", serverName);
                return;
            }
            catch (Exception ex)
            {
                retryAttempts++;
                _logger.LogError("Reconnect to {ServerName} failed: {Message}. Retry {Retry}/{Max}", serverName, ex.Message, retryAttempts, MaxRetryAttempts);
                await Task.Delay(TimeSpan.FromSeconds(ReconnectDelayInSeconds));
            }
        }
    }

    public void AddSendMessageHandler(Action<string> handler)
    {
        // Reserved for future implementation
    }
}
