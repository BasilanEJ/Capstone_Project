using OtpNet;

public static class TOTPHelper
{
    public static string GenerateSecret()
    {
        byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(secretKey);
    }

    public static string GetQRCodeUrl(string email, string secret, string issuer = "RRCManagementSystem")
    {
        return $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}";
    }

    public static bool VerifyCode(string secret, string code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1)); // allow ±1 step
    }
}
