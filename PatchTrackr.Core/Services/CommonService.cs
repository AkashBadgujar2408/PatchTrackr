
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace PatchTrackr.Core.Services;

public class CommonService(IHttpContextAccessor _httpContextAccessor, IConfiguration _configuration) : ICommonService
{
    private static readonly TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

    public string GetConnectionString()
    {
        return _configuration.GetConnectionString("PatchTrackrConn") ?? string.Empty;
    }

    public DateTime GetCurrentDateTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
    }

    public string GetIp()
    {
        if (_httpContextAccessor?.HttpContext == null)
            return string.Empty;

        var request = _httpContextAccessor.HttpContext.Request;

        // 1. Check X-Forwarded-For header (used by proxies)
        var forwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // The header can contain multiple IPs, take the first
            return forwardedFor.Split(',')[0].Trim();
        }

        // 2. Fallback: Use RemoteIpAddress
        var remoteIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            // Normalize IPv6 loopback (::1) to IPv4 (127.0.0.1)
            if (remoteIp.IsIPv4MappedToIPv6)
                remoteIp = remoteIp.MapToIPv4();

            return remoteIp.ToString();
        }
        return string.Empty;
    }

    public Guid GetLoggedInUserId()
    {
        if (_httpContextAccessor?.HttpContext == null)
            return Guid.Empty;

        var claim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
            return Guid.Empty;

        if (Guid.TryParse(claim.Value, out Guid userId))
            return userId;

        return Guid.Empty;
    }


    public string GetLoggedInUserName()
    {
        string username = string.Empty;

        if (_httpContextAccessor?.HttpContext != null)
        {
            username = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value ?? "User";
        }

        return username;
    }

    public UpdateAuditInfo GetUpdateAuditInfo()
    {
        UpdateAuditInfo auditInfo = new();
        auditInfo.LoggedInuserId = this.GetLoggedInUserId();
        auditInfo.LoggedInUserIp = this.GetIp();
        auditInfo.CurrentDateTime = DateTime.Now;

        return auditInfo;
    }

    public bool IsUserLoggedIn()
    {
        return _httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
