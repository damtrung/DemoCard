using APIClient.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using OtpSharp;
namespace APIClient.Helpers
{
    public static class OtpHelper
    {
        private const string OTP_HEADER = "SU-OTP";

        public static bool HasValidTotp(this HttpRequestMessage request, string key)
        {
            if (request.Headers.Contains(OTP_HEADER))
            {
                string otp = request.Headers.GetValues(OTP_HEADER).First();

                // We need to check the passcode against the past, current, and future passcodes

                if (!string.IsNullOrWhiteSpace(otp))
                {
                    byte[] secretKey = System.Text.Encoding.UTF8.GetBytes(key);
                    var topt = new Totp(secretKey, step: 30);
                    long timeWindowUsed;
                    bool isMatch = topt.VerifyTotp(otp, out timeWindowUsed, null);
                    if (isMatch)
                    {
                        return true;
                    }
                    //if (TimeSensitivePassCode.GetListOfOTPs(key).Any(t => t.Equals(otp)))
                    //{
                    //    return true;
                    //}
                }
            }
            return false;
        }
    }
}