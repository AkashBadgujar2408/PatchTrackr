
namespace PatchTrackr.Core.ServiceContracts;

public interface ICommonService
{
    bool IsUserLoggedIn();
    Guid GetLoggedInUserId();
    string GetLoggedInUserName();
    string GetIp();
    DateTime GetCurrentDateTime();
    string GetConnectionString();
    UpdateAuditInfo GetUpdateAuditInfo();
}
