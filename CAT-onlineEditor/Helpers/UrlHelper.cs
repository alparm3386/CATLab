using CAT.Enums;
using NuGet.Packaging.Signing;

namespace CAT.Helpers
{
    public static class UrlHelper
    {
        public static string CreateOnlineEditorUrl(int idJob, OEMode mode)
        {
            //random for salt
            var random = new Random((int)DateTime.Now.Ticks);
            var sUrlParams = $"salt1={random.Next()}&idJob={idJob}&mode={(int)mode}&salt2={random.Next()}";
            var encryptedParams = EncryptionHelper.EncryptString(sUrlParams);
            var sUrl = "http://localhost:3000/?" + System.Net.WebUtility.UrlEncode(encryptedParams);
            return sUrl;
        }
    }
}
