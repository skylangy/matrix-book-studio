using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ServerInfoController : ControllerBase
{
    [HttpGet("tag")]
    public IActionResult GetServerInfo()
    {
        string serverIP = GetLocalIPAddress();
        return Ok(new { ip = serverIP });
    }

    private static string GetLocalIPAddress()
    {
        string localIP = "Unknown";
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4 only
                {
                    var ipAddr = ip.ToString();
                    localIP = ipAddr;
                    break;
                }
            }
        }
        catch
        {
            // Handle exceptions if DNS resolution fails
        }
        return localIP;
    }
}
