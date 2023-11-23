﻿using CAT.Enums;
using NuGet.Packaging.Signing;

namespace CAT.Helpers
{
    public static class UrlHelper
    {
        public static string CreateOnlineEditorUrl(string baseUrl, int jobId, OEMode mode)
        {
            //random for salt
            var random = new Random((int)DateTime.Now.Ticks);
            var sUrlParams = $"salt1={random.Next()}&jobId={jobId}&mode={(int)mode}&salt2={random.Next()}";
            var encryptedParams = EncryptionHelper.EncryptString(sUrlParams);

            var sUrl = baseUrl + "?" + System.Net.WebUtility.UrlEncode(encryptedParams);
            return sUrl;
        }
    }
}
