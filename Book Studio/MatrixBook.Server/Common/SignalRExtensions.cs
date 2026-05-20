using AudioBookStudio.Common.Models;
using MatrixBook.Server.Services;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace MatrixBook.Server.Common;

public static class SignalRExtensions
{
    public static async Task Publish(this IHubContext<WorkProgressHub> hubContext, WorkProgress workProgress)
    {
        if (workProgress == null)
        {
            return;
        }
        await hubContext.Clients.All.SendAsync("WorkProgressChanged", workProgress);
    }

    public static async Task RunCommand(this IHubContext<CommandHub> hubContext, CommandModel command)
    {
        if (command == null)
        {
            return;
        }

        var payload = JsonSerializer.Serialize(command);
        await hubContext.Clients.All.SendAsync("ExecuteCommand", payload);
    }
}
