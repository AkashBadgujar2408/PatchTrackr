using System.Security.Cryptography;
using System.Text;
using FluentResults;

public enum EncryptionType : int
{
    AesGcm = 1,
    ChaCha20Poly1305 = 2,
    AesCbcHmac = 3
}

/// <summary>
/// Multi-algorithm, randomized, reversible encryption service for .NET 8.
/// - Per-message random salt + PBKDF2 iterations to derive a unique key
/// - Randomly chosen algorithm among: AES-GCM, ChaCha20-Poly1305, AES-CBC+HMAC
/// - Output format: iterations|alg|saltBase64|payloadBase64
///   where payload layout depends on algorithm:
///     AES-GCM         => nonce(12) || tag(16) || ciphertext
///     ChaCha20-Poly1305 => nonce(12) || tag(16) || ciphertext
///     AES-CBC+HMAC    => iv(16) || hmac(32) || ciphertext
/// - Returns FluentResults.Result for success/failure with clear messages
/// </summary>
public class CryptoService : ICryptoService
{
    // MASTER KEY: provide securely (KeyVault / env var). Recommend 32 bytes.
    private readonly byte[] _masterKey = Convert.FromBase64String("q+8K6V8hK2I6Jz0v3Fj3oNQZQK8E3R1A5bVvYpTt+1s=");

    public Result<string> Encrypt(string plaintext)
    {
        try
        {
            if (string.IsNullOrEmpty(plaintext))
                return Result.Fail("Plaintext cannot be null or empty.");

            // 1) Random PBKDF2 iteration count per message
            int iterations = RandomNumberGenerator.GetInt32(10_000, 50_000);

            // 2) Random algorithm selection
            var algValues = Enum.GetValues(typeof(EncryptionType)).Cast<EncryptionType>().ToArray();
            EncryptionType chosen = algValues[RandomNumberGenerator.GetInt32(0, algValues.Length)];

            // 3) Random salt (16 bytes)
            byte[] salt = RandomBytes(16);

            // 4) Derive per-message key (32 bytes)
            byte[] derivedKey = new Rfc2898DeriveBytes(_masterKey, salt, iterations, HashAlgorithmName.SHA256).GetBytes(32);

            // 5) Encrypt according to chosen algorithm
            byte[] payloadBytes = chosen switch
            {
                EncryptionType.AesGcm => EncryptAesGcm(Encoding.UTF8.GetBytes(plaintext), derivedKey).Value,
                EncryptionType.ChaCha20Poly1305 => EncryptChaCha20Poly1305(Encoding.UTF8.GetBytes(plaintext), derivedKey).Value,
                EncryptionType.AesCbcHmac => EncryptAesCbcHmac(Encoding.UTF8.GetBytes(plaintext), derivedKey).Value,
                _ => throw new NotSupportedException("Unknown algorithm")
            };

            // 6) Compose final string: iterations|alg|saltBase64|payloadBase64
            string final = $"{iterations}|{(int)chosen}|{Convert.ToBase64String(salt)}|{Convert.ToBase64String(payloadBytes)}";
            return Result.Ok(final);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Encryption failed: {ex.Message}");
        }
    }

    public Result<string> Decrypt(string encryptedPayload)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(encryptedPayload))
                return Result.Fail("Encrypted payload cannot be null or empty.");

            // expected format: iterations|alg|saltBase64|payloadBase64
            var parts = encryptedPayload.Split('|');
            if (parts.Length != 4)
                return Result.Fail("Invalid encrypted payload format.");

            if (!int.TryParse(parts[0], out int iterations))
                return Result.Fail("Invalid iterations field.");

            if (!int.TryParse(parts[1], out int algInt) || !Enum.IsDefined(typeof(EncryptionType), algInt))
                return Result.Fail("Invalid algorithm field.");

            var alg = (EncryptionType)algInt;

            byte[] salt;
            byte[] payload;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                payload = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return Result.Fail("Salt or payload is not valid Base64.");
            }

            // Derive same per-message key
            byte[] derivedKey = new Rfc2898DeriveBytes(_masterKey, salt, iterations, HashAlgorithmName.SHA256).GetBytes(32);

            string plaintext = alg switch
            {
                EncryptionType.AesGcm => DecryptAesGcm(payload, derivedKey).IsFailed ? throw new CryptographicException("AES-GCM decryption failed.") : DecryptAesGcm(payload, derivedKey).Value,
                EncryptionType.ChaCha20Poly1305 => DecryptChaCha20Poly1305(payload, derivedKey).IsFailed ? throw new CryptographicException("ChaCha20-Poly1305 decryption failed.") : DecryptChaCha20Poly1305(payload, derivedKey).Value,
                EncryptionType.AesCbcHmac => DecryptAesCbcHmac(payload, derivedKey).IsFailed ? throw new CryptographicException("AES-CBC+HMAC decryption failed.") : DecryptAesCbcHmac(payload, derivedKey).Value,
                _ => throw new NotSupportedException("Unknown algorithm")
            };

            return Result.Ok(plaintext);
        }
        catch (CryptographicException cex)
        {
            return Result.Fail($"Decryption failed (crypto): {cex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Decryption failed: {ex.Message}");
        }
    }

    // -------------------------- AES-GCM (AEAD) --------------------------
    // payload layout: nonce(12) || tag(16) || ciphertext

    private Result<byte[]> EncryptAesGcm(byte[] plain, byte[] key)
    {
        try
        {
            if (!(key.Length == 16 || key.Length == 24 || key.Length == 32))
                return Result.Fail<byte[]>("Invalid key length for AES-GCM.");

            byte[] nonce = RandomBytes(12); // 96-bit recommended
            byte[] cipher = new byte[plain.Length];
            byte[] tag = new byte[16]; // 128-bit tag

            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Encrypt(nonce, plain, cipher, tag, null);
            }

            byte[] payload = CombineBytes(nonce, tag, cipher);
            return Result.Ok(payload);
        }
        catch (Exception ex)
        {
            return Result.Fail<byte[]>($"AES-GCM encryption failed: {ex.Message}");
        }
    }

    private Result<string> DecryptAesGcm(byte[] payload, byte[] key)
    {
        try
        {
            if (payload.Length < 12 + 16)
                return Result.Fail<string>("AES-GCM payload too short.");

            byte[] nonce = payload.Take(12).ToArray();
            byte[] tag = payload.Skip(12).Take(16).ToArray();
            byte[] cipher = payload.Skip(28).ToArray();

            byte[] plain = new byte[cipher.Length];
            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Decrypt(nonce, cipher, tag, plain, null);
            }

            return Result.Ok(Encoding.UTF8.GetString(plain));
        }
        catch (CryptographicException)
        {
            return Result.Fail<string>("AES-GCM authentication failed (tag mismatch).");
        }
        catch (Exception ex)
        {
            return Result.Fail<string>($"AES-GCM decryption failed: {ex.Message}");
        }
    }

    // -------------------- ChaCha20-Poly1305 (AEAD) -----------------------
    // payload layout: nonce(12) || tag(16) || ciphertext
    // System.Security.Cryptography.ChaCha20Poly1305 is available in modern .NET (6+ / 8)

    private Result<byte[]> EncryptChaCha20Poly1305(byte[] plain, byte[] key)
    {
        try
        {
            if (key.Length != 32)
                return Result.Fail<byte[]>("ChaCha20-Poly1305 requires a 32-byte key.");

            byte[] nonce = RandomBytes(12);
            byte[] cipher = new byte[plain.Length];
            byte[] tag = new byte[16];

            using (var ch = new ChaCha20Poly1305(key))
            {
                ch.Encrypt(nonce, plain, cipher, tag, null);
            }

            byte[] payload = CombineBytes(nonce, tag, cipher);
            return Result.Ok(payload);
        }
        catch (Exception ex)
        {
            return Result.Fail<byte[]>($"ChaCha20-Poly1305 encryption failed: {ex.Message}");
        }
    }

    private Result<string> DecryptChaCha20Poly1305(byte[] payload, byte[] key)
    {
        try
        {
            if (payload.Length < 12 + 16)
                return Result.Fail<string>("ChaCha20-Poly1305 payload too short.");

            byte[] nonce = payload.Take(12).ToArray();
            byte[] tag = payload.Skip(12).Take(16).ToArray();
            byte[] cipher = payload.Skip(28).ToArray();

            byte[] plain = new byte[cipher.Length];
            using (var ch = new ChaCha20Poly1305(key))
            {
                ch.Decrypt(nonce, cipher, tag, plain, null);
            }

            return Result.Ok(Encoding.UTF8.GetString(plain));
        }
        catch (CryptographicException)
        {
            return Result.Fail<string>("ChaCha20-Poly1305 authentication failed (tag mismatch).");
        }
        catch (Exception ex)
        {
            return Result.Fail<string>($"ChaCha20-Poly1305 decryption failed: {ex.Message}");
        }
    }

    // -------------------- AES-CBC + HMAC (Encrypt-then-MAC) ----------------
    // payload layout: iv(16) || hmac(32) || ciphertext
    // HMAC-SHA256 with derived key

    private Result<byte[]> EncryptAesCbcHmac(byte[] plain, byte[] key)
    {
        try
        {
            // Use key material as: first 32 bytes -> AES key, second 32 bytes (HMAC) -> HMAC key
            // If derived key is only 32 bytes, expand via HKDF-like simple derivation for HMAC key.
            byte[] aesKey = key;
            byte[] hmacKey = HkdfExpand(key, 32); // produce 32-byte HMAC key

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();
            byte[] iv = aes.IV;

            byte[] cipher;
            using (var encryptor = aes.CreateEncryptor())
            {
                cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);
            }

            byte[] ivAndCipher = Combine(iv, cipher);

            using var hmac = new HMACSHA256(hmacKey);
            byte[] tag = hmac.ComputeHash(ivAndCipher); // 32 bytes

            byte[] payload = Combine(iv, tag, cipher); // iv || tag || cipher
            return Result.Ok(payload);
        }
        catch (Exception ex)
        {
            return Result.Fail<byte[]>($"AES-CBC+HMAC encryption failed: {ex.Message}");
        }
    }

    private Result<string> DecryptAesCbcHmac(byte[] payload, byte[] key)
    {
        try
        {
            if (payload.Length < 16 + 32)
                return Result.Fail<string>("AES-CBC+HMAC payload too short.");

            byte[] iv = payload.Take(16).ToArray();
            byte[] tag = payload.Skip(16).Take(32).ToArray();
            byte[] cipher = payload.Skip(48).ToArray();

            byte[] aesKey = key;
            byte[] hmacKey = HkdfExpand(key, 32);

            // Validate HMAC over iv||cipher
            byte[] ivAndCipher = Combine(iv, cipher);
            using var hmac = new HMACSHA256(hmacKey);
            byte[] computed = hmac.ComputeHash(ivAndCipher);

            if (!CryptographicOperations.FixedTimeEquals(tag, computed))
                return Result.Fail<string>("AES-CBC+HMAC authentication failed (HMAC mismatch).");

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] plain;
            using (var decryptor = aes.CreateDecryptor())
            {
                plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            }

            return Result.Ok(Encoding.UTF8.GetString(plain));
        }
        catch (CryptographicException)
        {
            return Result.Fail<string>("AES-CBC decryption failed (bad padding or auth).");
        }
        catch (Exception ex)
        {
            return Result.Fail<string>($"AES-CBC+HMAC decryption failed: {ex.Message}");
        }
    }

    // -------------------- Helpers --------------------

    private static byte[] RandomBytes(int len)
    {
        var b = new byte[len];
        RandomNumberGenerator.Fill(b);
        return b;
    }

    private static byte[] Combine(params byte[][] parts)
    {
        int total = parts.Sum(p => p.Length);
        var outb = new byte[total];
        int pos = 0;
        foreach (var p in parts)
        {
            Buffer.BlockCopy(p, 0, outb, pos, p.Length);
            pos += p.Length;
        }
        return outb;
    }

    private static byte[] CombineBytes(byte[] a, byte[] b, byte[] c)
        => Combine(a, b, c);

    // Simple HKDF-ish expand using HMAC-SHA256 (not full HKDF; fine for deriving HMAC key from derived key here)
    private static byte[] HkdfExpand(byte[] keyMaterial, int outputLen)
    {
        // PRK = HMAC_SHA256(salt=empty, key=keyMaterial) -> just use keyMaterial as key for HMAC with zero salt
        using var hmacPrk = new HMACSHA256(keyMaterial);
        byte[] prk = hmacPrk.ComputeHash(Array.Empty<byte>());

        byte[] result = new byte[outputLen];
        byte[] previous = Array.Empty<byte>();
        int bytesGenerated = 0;
        byte counter = 1;

        while (bytesGenerated < outputLen)
        {
            using var hmac = new HMACSHA256(prk);
            hmac.TransformBlock(previous, 0, previous.Length, null, 0);
            hmac.TransformBlock(Array.Empty<byte>(), 0, 0, null, 0);
            hmac.TransformFinalBlock(new[] { counter }, 0, 1);
            byte[] block = hmac.Hash;
            int take = Math.Min(block.Length, outputLen - bytesGenerated);
            Array.Copy(block, 0, result, bytesGenerated, take);
            bytesGenerated += take;
            previous = block;
            counter++;
        }

        return result;
    }
}