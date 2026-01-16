using System;

namespace JsonCrudApp.Services
{
    public class OtpService
    {
        public string GenerateOtp()
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                rng.GetBytes(data);
                int value = BitConverter.ToInt32(data, 0) & Int32.MaxValue;
                return (value % 900000 + 100000).ToString();
            }
        }
        public bool IsValid(string inputOtp, string storedOtp, DateTime expiry)
        {
            return inputOtp == storedOtp && DateTime.Now <= expiry;
        }
    }
}
