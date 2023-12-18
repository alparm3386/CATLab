using System.Web;

namespace CAT.Helpers
{
    public static class QueryHelper
    {
        public static string? GetQuerystringParameter(String queryString, String key)
        {
            if (String.IsNullOrEmpty(queryString))
                return null;

            //try to decript the query string first
            String sDecripted = EncryptionHelper.DecryptString(queryString);
            if (!String.IsNullOrEmpty(sDecripted))
                return HttpUtility.ParseQueryString(sDecripted)[key];
            else
                return HttpUtility.ParseQueryString(queryString)[key];
        }

        public static int GetQuerystringIntParameter(String queryString, String key)
        {
            return int.Parse(GetQuerystringParameter(queryString, key)!);
        }
    }
}
