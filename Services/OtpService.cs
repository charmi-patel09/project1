using System;

namespace JsonCrudApp.Services
{
    public class OtpService
    {
        public string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public bool IsValid(string inputOtp, string storedOtp, DateTime expiry)
        {
            return inputOtp == storedOtp && DateTime.Now <= expiry;
        }
    }
}
