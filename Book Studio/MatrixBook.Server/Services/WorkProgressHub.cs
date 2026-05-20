using AudioBookStudio.Common.Models;
using Microsoft.AspNetCore.SignalR;

namespace MatrixBook.Server.Services;

public class WorkProgressHub : Hub
{
    public async Task PublishWorkProgress(WorkProgress workProgress)
    {
        await Clients.All.SendAsync("WorkProgressChanged", workProgress);
    }
}
