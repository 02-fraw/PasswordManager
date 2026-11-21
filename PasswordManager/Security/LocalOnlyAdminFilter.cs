using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PasswordManager.Security;

public sealed class LocalOnlyAdminFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
        var localIp = context.HttpContext.Connection.LocalIpAddress;

        bool isLocal = remoteIp is null
            || IPAddress.IsLoopback(remoteIp)
            || (localIp is not null && remoteIp.Equals(localIp));

        if (!isLocal)
        {
            context.Result = new ForbidResult();
        }
    }
}
