
using System.Net;

namespace Gibbon.Git.Server.Helpers;

public static class RequestHelper
{
    public static bool IsLocalRequest(this HttpContext context)
    {
        var connection = context.Connection;

        // Wenn die Remote-IP-Adresse nicht gesetzt ist, handelt es sich um eine lokale Anfrage
        if (connection.RemoteIpAddress == null)
        {
            return true;
        }

        // Vergleiche die Remote-IP-Adresse mit der Local-IP-Adresse (localhost)
        if (connection.RemoteIpAddress.Equals(connection.LocalIpAddress))
        {
            return true;
        }

        // Prüfe auf IPv4-Loopback-Adresse (127.0.0.1) oder IPv6-Loopback-Adresse (::1)
        if (IPAddress.IsLoopback(connection.RemoteIpAddress))
        {
            return true;
        }

        return false;
    }

}