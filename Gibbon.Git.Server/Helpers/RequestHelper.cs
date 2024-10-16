
using System.Net;

namespace Gibbon.Git.Server.Helpers;

public static class RequestHelper
{
    public static bool IsLocalRequest(this HttpContext context)
    {
        var connection = context.Connection;

        if (connection.RemoteIpAddress == null)
        {
            return true;
        }

        if (connection.RemoteIpAddress.Equals(connection.LocalIpAddress))
        {
            return true;
        }

        if (IPAddress.IsLoopback(connection.RemoteIpAddress))
        {
            return true;
        }

        return false;
    }

}
