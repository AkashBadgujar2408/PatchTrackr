
namespace PatchTrackr.Core.DTOs;

public class UpdateAuditInfo
{
    public Guid LoggedInuserId { get; set; }
    public DateTime CurrentDateTime { get; set; }
    public string? LoggedInUserIp { get; set; }
}