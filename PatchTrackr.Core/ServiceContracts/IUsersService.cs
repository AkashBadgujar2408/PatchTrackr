
using FluentResults;
using PatchTrackr.Core.Models;

namespace PatchTrackr.Core.ServiceContracts;

    public interface IUsersService
    {
    Task<Result<MUser?>> LoginUserAsync(LoginDTO loginDTO);
    Task<List<MUser>> GetAllUsersAsync();
    Task<List<MUser>> GetActiveUsersAsync();
    Task<bool> IsValidUser(string username);
    Task<bool> AddUpdateUserAsync(UserDTO userDTO);
    Task<MUser?> GetUserByUserNameAsync(string userName);
}
