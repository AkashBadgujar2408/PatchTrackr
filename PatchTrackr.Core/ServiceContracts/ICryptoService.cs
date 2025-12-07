using FluentResults;

namespace PatchTrackr.Core.ServiceContracts;
public interface ICryptoService
{
    Result<string> Encrypt(string plaintext);
    Result<string> Decrypt(string encryptedPayload);
}
