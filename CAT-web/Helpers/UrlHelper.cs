using CAT_web.Enums;
using NuGet.Packaging.Signing;

namespace CAT_web.Helpers
{
    public static class UrlHelper
    {
        public static string CreateOnlineEditorUrl(int idJob, OEMode mode)
        {
            //random for salt
            var random = new Random((int)DateTime.Now.Ticks);
            var sUrl = $"salt1={random.Next()}&idJob={idJob}&mode={mode}&salt2={random.Next()}";
            sUrl = EncryptionHelper.EncryptString(sUrl);
            return sUrl;
        }
    }
}
