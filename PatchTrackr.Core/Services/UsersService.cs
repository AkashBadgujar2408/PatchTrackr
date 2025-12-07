

using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Dapper;

namespace PatchTrackr.Core.Services;

public class UsersService(AppDbContext _dbContext, IHttpContextAccessor _httpContextAccessor,
    ICryptoService _cryptoService, ICommonService _commonService) : IUsersService
{
    public async Task<bool> AddUpdateUserAsync(UserDTO userDTO)
    {
        using var connection = new SqlConnection(_commonService.GetConnectionString());
        UpdateAuditInfo auditInfo = _commonService.GetUpdateAuditInfo();
        var parameters = new
        {
            userDTO.UserName,
            userDTO.Email,
            userDTO.FullName,
            userDTO.PhoneNo,
            PasswordHash = _cryptoService.Encrypt(userDTO.Password).ValueOrDefault,
            IsActive = userDTO.IsActive,
            UpdatedBy = auditInfo.LoggedInuserId,
            UpdatedIp = auditInfo.LoggedInUserIp
        };
        int recordsAffected = (await connection.ExecuteAsync("sp_AddUpdateUser", parameters));

        return recordsAffected > 0;
    }

    public async Task<List<MUser>> GetActiveUsersAsync()
    {
        return await _dbContext.MUsers.Where(u => u.IsActive).ToListAsync();
    }

    public async Task<List<MUser>> GetAllUsersAsync()
    {
        return await _dbContext.MUsers.ToListAsync();
    }

    public async Task<MUser?> GetUserByUserNameAsync(string userName)
    {
        MUser? user = await _dbContext.MUsers.FirstOrDefaultAsync(u => u.UserName!.Trim() == userName.Trim());
        return user;
    }

    public async Task<bool> IsValidUser(string username)
    {
        bool isValidUser = false;
        isValidUser = await _dbContext.MUsers.AnyAsync(u => u.UserName!.Trim() == username.Trim());

        if (!isValidUser)
        {

        }
        return isValidUser;
    }

    public async Task<Result<MUser?>> LoginUserAsync(LoginDTO loginDTO)
    {
        MUser? user = await _dbContext.MUsers.FirstOrDefaultAsync(ad => ad.UserName!.Trim() == loginDTO.Username!.Trim());
        if (user == null)
        {
            return Result.Fail("Username doesn't exist.");
        }
        if (!user.IsActive)
        {
            return Result.Fail("User is inactive. Login failed.");
        }
        //Encryption/Decryption logic pending
        Result<string> decryptionResult = _cryptoService.Decrypt(user.PasswordHash);
        if (decryptionResult.IsFailed)
        {
            return Result.Fail("Error decrypting password.");
        }
        if (loginDTO.Password != decryptionResult.Value)
        {
            return Result.Fail("Invalid password.");
        }

        var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                                new Claim("UserName", user.UserName),
                                new Claim("DisplayName", user.FullName),
                            };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return user;
    }
}
