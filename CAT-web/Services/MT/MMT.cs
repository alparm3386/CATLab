using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Transactions;
using System.Web;
using System.Xml;
using System.Security;

namespace CAT_web.Services.MT
{
    public class MMT : IMachineTranslator
    {
        public static int ID_GENERAL_TM = 11471;
        public static int MT_MAX_LENGTH = 4000;
        public enum MemoryType
        {
            TM = 0,
            TB = 1
        }

        public static String[] SUPPORTED_LANGUAGES = new String[] { "sq", "ar", "bs", "bg", "ca", "zh-cn", "zh-tw", "hr", "cs", "da", "nl", "en", "et", "fi", "fr", "de",
            "el", "he", "hu", "is", "id", "it", "ja", "ko", "lv", "lt", "mk", "ms", "mt", "nb", "nn", "pl", "pt-pt", "pt-br", "ro", "ru", "sr", "sk", "sl", "es-es",
            "es-mx", "sv", "th", "tr", "uk", "vi" };

        private static Dictionary<String, String> TM_LOOKUP = new Dictionary<String, String>();
        private static String API_KEY = "1E9A769A-A64D-4FDD-817A-76D715722FC4";
        private static String HOST = "http://api.modernmt.com";

        public String DetectLanguage(String sText)
        {
            throw new Exception("Not implemented.");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public String GetContextVector(String sFrom, String sTo, String sTextExtract)
        {
            try
            {
                if (sFrom != "en" && sTo != "en")
                    return null;

                sFrom = ConvertLanguageCode(sFrom);
                sTo = ConvertLanguageCode(sTo);

                String sContextVector = CalculateContextVector(sFrom, sTo, sTextExtract, null);

                sContextVector = String.IsNullOrEmpty(sContextVector) ? ID_GENERAL_TM.ToString() + ":1" : sContextVector;

                return sContextVector;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static String CalculateContextVector(String sFrom, String sTo, String sText, String sHints)
        {
            try
            {
                //set the context vector
                String sUrl = HOST + "/context-vector";

                WebClient webClient = new WebClient();
                webClient.Headers["MMT-ApiKey"] = API_KEY;
                webClient.Headers["X-HTTP-Method-Override"] = "GET";
                webClient.Headers["MMT-Platform"] = "translatemedia.com";
                webClient.Headers["MMT-PluginVersion"] = "1.0";
                webClient.Headers["MMT-PlatformVersion"] = "1.0";
                webClient.Encoding = Encoding.UTF8;

                //[AM 2020.03.13] "request entity too large" error from MMT. Let's trim the text
                sText = sText.Substring(0, Math.Min(sText.Length, 2500));

                var reqparm = new NameValueCollection();
                reqparm.Add("text", sText);
                reqparm.Add("hints", sHints);
                reqparm.Add("source", sFrom);
                reqparm.Add("targets", sTo);
                byte[] responsebytes = webClient.UploadValues(sUrl, "POST", reqparm);
                var sResponse = Encoding.Default.GetString(responsebytes);

                var o = (JToken)JsonConvert.DeserializeObject(sResponse);
                var sContextVector = o["data"]["vectors"][sTo].ToString();

                return sContextVector;
            }
            catch (WebException ex)
            {
                var responseStream = (MemoryStream)ex.Response.GetResponseStream();
                String responseJson = Encoding.ASCII.GetString(responseStream.ToArray());
                //throw;
            }
            catch (Exception ex)
            {
                //throw;
            }

            return "";
        }

        public static bool IsLanguagePairSupported(String sSourceLangCode, String sTargetLangCode)
        {
            String sourceLang = ConvertLanguageCode(sSourceLangCode);
            String targetLang = ConvertLanguageCode(sTargetLangCode);

            return SUPPORTED_LANGUAGES.Contains(sourceLang.ToLower()) && SUPPORTED_LANGUAGES.Contains(targetLang.ToLower()) && sourceLang != targetLang;
        }

        public static String ConvertLanguageCode(String sLangCodeISO639_1)
        {
            sLangCodeISO639_1 = sLangCodeISO639_1.ToLower();
            if (sLangCodeISO639_1 == "pt") return "pt-PT";
            if (sLangCodeISO639_1 == "sh") return "sr"; //Serbian(Latin) -> MMT (Latin)
            if (sLangCodeISO639_1 == "sr") return "??"; //Serbian (Cyrillic) -> Dummy, not supported by MMT
            if (sLangCodeISO639_1 == "pt-br") return "pt-BR";
            if (sLangCodeISO639_1 == "zh-cn") return "zh-CN";
            if (sLangCodeISO639_1 == "zh-tw") return "zh-TW";
            if (sLangCodeISO639_1 == "zh-hk") return "zh-TW";
            if (sLangCodeISO639_1 == "no") return "nb";
            if (sLangCodeISO639_1 == "es") return "es-ES";
            if (sLangCodeISO639_1 == "es-ar" || sLangCodeISO639_1 == "es-bo" || sLangCodeISO639_1 == "es-cl" || sLangCodeISO639_1 == "es-co" || sLangCodeISO639_1 == "es-cr" || sLangCodeISO639_1 == "es-do" || sLangCodeISO639_1 == "es-ec" ||
                sLangCodeISO639_1 == "es-sv" || sLangCodeISO639_1 == "es-gt" || sLangCodeISO639_1 == "es-hn" || sLangCodeISO639_1 == "es-bo" || sLangCodeISO639_1 == "es-mx" || sLangCodeISO639_1 == "es-ni" || sLangCodeISO639_1 == "es-pa" ||
                sLangCodeISO639_1 == "es-py" || sLangCodeISO639_1 == "es-pe" || sLangCodeISO639_1 == "es-pr" || sLangCodeISO639_1 == "es-uy" || sLangCodeISO639_1 == "es-us" || sLangCodeISO639_1 == "es-ve") return "es-MX";

            //filipino uses ISO639-2
            if (sLangCodeISO639_1 == "fil")
                return sLangCodeISO639_1;

            return sLangCodeISO639_1.Substring(0, 2).ToLower();
        }

        public static String ConvertLanguageCodeISO639_2(String sLangCodeISO639_2)
        {
            sLangCodeISO639_2 = sLangCodeISO639_2.ToLower();
            if (sLangCodeISO639_2 == "por") return "por-PT";
            if (sLangCodeISO639_2 == "por-br") return "por-BR";
            if (sLangCodeISO639_2 == "zho-cn") return "zho-CN";
            if (sLangCodeISO639_2 == "zho-tw") return "zho-TW";
            if (sLangCodeISO639_2 == "zho-hk") return "zho-TW";
            if (sLangCodeISO639_2 == "spa") return "es-ES";
            if (sLangCodeISO639_2 == "spa-ar" || sLangCodeISO639_2 == "spa-bo" || sLangCodeISO639_2 == "spa-cl" || sLangCodeISO639_2 == "spa-co" || sLangCodeISO639_2 == "spa-cr" || sLangCodeISO639_2 == "spa-do" || sLangCodeISO639_2 == "spa-ec" ||
                sLangCodeISO639_2 == "spa-sv" || sLangCodeISO639_2 == "spa-gt" || sLangCodeISO639_2 == "spa-hn" || sLangCodeISO639_2 == "spa-bo" || sLangCodeISO639_2 == "spa-mx" || sLangCodeISO639_2 == "spa-ni" || sLangCodeISO639_2 == "spa-pa" ||
                sLangCodeISO639_2 == "spa-py" || sLangCodeISO639_2 == "spa-pe" || sLangCodeISO639_2 == "spa-pr" || sLangCodeISO639_2 == "spa-uy" || sLangCodeISO639_2 == "spa-us" || sLangCodeISO639_2 == "spa-ve") return "spa-MX";

            //filipino uses ISO639-2
            if (sLangCodeISO639_2 == "fil")
                return sLangCodeISO639_2;

            return sLangCodeISO639_2.Substring(0, 3).ToLower();
        }

        protected static String GoogleTagsToMTTags(String sText)
        {
            //convert google tags to xml tags
            sText = SecurityElement.Escape(sText);
            sText = Regex.Replace(sText, "\\{(\\d+)\\}", "<b id=\"$1\"> </b>");
            sText = Regex.Replace(sText, "\\{/(\\d+)\\}", "<i id=\"$1\"> </i>");
            sText = Regex.Replace(sText, "\\{(\\d+)/\\}", "<u id=\"$1\"> </u>");

            return sText;
        }

        protected static String MTTagsToGoogleTags(String sText)
        {
            sText = Regex.Replace(sText, "<b id=[\"'](\\d+)[\"']>\\s*?</b>", "{$1}");
            sText = Regex.Replace(sText, "<b id=[\"'](\\d+)[\"']>", "{$1}");
            sText = Regex.Replace(sText, "</b id=[\"'](\\d+)[\"']\\s*?>", "");
            sText = Regex.Replace(sText, "<i id=[\"'](\\d+)[\"']>\\s*?</i>", "{/$1}");
            sText = Regex.Replace(sText, "<i id=[\"'](\\d+)[\"']>", "{/$1}");
            sText = Regex.Replace(sText, "</i id=[\"'](\\d+)[\"']\\s*?>", "");
            sText = Regex.Replace(sText, "<u id=[\"'](\\d+)[\"']>\\s*?</u>", "{$1/}");
            sText = Regex.Replace(sText, "<u id=[\"'](\\d+)[\"']>", "{$1/}");
            sText = Regex.Replace(sText, "</u id=[\"'](\\d+)[\"']\\s*?>", "");

            return HttpUtility.HtmlDecode(sText);
        }

        public String Translate(String sText, String sFrom, String sTo, Object mtParams)
        {
            if (String.IsNullOrEmpty(sText))
                return "";

            String sContextVector = mtParams != null ? (String)mtParams : "";
            sText = GoogleTagsToMTTags(sText);
            //give three shot
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    sFrom = ConvertLanguageCode(sFrom);
                    sTo = ConvertLanguageCode(sTo);

                    //check if the source and target ar same
                    if (sFrom == sTo)
                        return sText;

                    //set the context vector
                    String sUrl = HOST + "/translate?variant=true&source={0}&target={1}&q={2}";
                    sUrl = String.Format(sUrl, sFrom, sTo, HttpUtility.UrlEncode(sText));
                    if (!String.IsNullOrEmpty(sContextVector))
                        sUrl += "&context_vector=" + sContextVector;

                    WebClient webClient = new WebClient();
                    webClient.Headers["MMT-ApiKey"] = API_KEY;
                    webClient.Headers["MMT-Platform"] = "translatemedia.com";
                    webClient.Headers["MMT-PluginVersion"] = "1.0";
                    webClient.Headers["MMT-PlatformVersion"] = "1.0";
                    webClient.Encoding = Encoding.UTF8;
                    String sTranslation = webClient.DownloadString(sUrl);

                    var o = (Newtonsoft.Json.Linq.JToken)JsonConvert.DeserializeObject(sTranslation);
                    sTranslation = o["data"]["translation"].ToString();

                    sTranslation = MTTagsToGoogleTags(sTranslation);
                    return sTranslation;
                }
                catch (WebException ex)
                {
                    try
                    {
                        if (ex.Message.Contains("Payload Too Large"))
                            return sText;
                        var responseStream = (MemoryStream)ex.Response.GetResponseStream();
                        String responseJson = Encoding.ASCII.GetString(responseStream.ToArray());
                        if (responseJson.Contains("Language pair not supported"))
                            break;
                        Thread.Sleep(5000);
                    }
                    catch (Exception e)
                    {
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(5000);
                }
                finally
                {
                }
            }

            throw new Exception("MMT ERROR");
        }

        private void TranslateOneByOne(List<Translatable> translatables, string sFrom, string sTo, Object mtParams)
        {
            //return TranslateContents(mtContents, sFrom, sTo, mtParams, MT_MAX_LENGTH, TranslateArray);
            Dictionary<int, String> aRet = new Dictionary<int, String>();
            String[] aMemoryNames = null;
            if (mtParams != null)
                aMemoryNames = (String[])mtParams;

            int segmentsToExtract = Math.Min(translatables.Count, 100);
            var sbTextExtract = new StringBuilder();
            for (int i = 0; i < segmentsToExtract; i++)
                sbTextExtract.Append(GoogleTagsToMTTags(translatables[i].source) + "\n");

            String sContextVector = GetContextVector(sFrom, sTo, sbTextExtract.ToString());
            var lookupTable = new Dictionary<String, String>();

            foreach (var translatable in translatables)
            {
                if (lookupTable.ContainsKey(translatable.source))
                {
                    translatable.target = lookupTable[translatable.source];
                    continue;
                }

                //do the translation
                String sTranslation = Translate(translatable.source, sFrom, sTo, sContextVector);
                translatable.target = sTranslation;
                lookupTable.Add(translatable.source, translatable.target);
            }
        }

        public void Translate(List<Translatable> translatables, string sFrom, string sTo, Object? mtParams)
        {
            //var useBatch = AppConfig.GetValue("USE_MF_TRANSLATE_ARRAY");
            //if (useBatch != null && useBatch == "1")
            //    return TranslateInBatch(mtContents, sFrom, sTo, mtParams, withRisk);
            //else
            //    return TranslateOneByOne(mtContents, sFrom, sTo, mtParams, withRisk);

            TranslateOneByOne(translatables, sFrom, sTo, mtParams);
        }

        //public List<MTContent> TranslateInBatch(List<MTContent> mtContents, string sFrom, string sTo, Object mtParams, bool withRisk)
        //{
        //    String[] aMemoryNames = null;
        //    if (mtParams != null)
        //        aMemoryNames = (String[])mtParams;

        //    int segmentsToExtract = Math.Min(mtContents.Count, 100);
        //    var sbTextExtract = new StringBuilder();
        //    for (int i = 0; i < segmentsToExtract; i++)
        //        sbTextExtract.Append(GoogleTagsToMTTags(mtContents[i].source) + "\n");

        //    String sContextVector = GetContextVector(sFrom, sTo, aMemoryNames, sbTextExtract.ToString());

        //    return TranslateContents(mtContents, sFrom, sTo, sContextVector, MT_MAX_LENGTH, withRisk, TranslateArray);
        //}

        //public String[] TranslateArray(String[] aSourceContents, String sFrom, String sTo, Object mtParams)
        //{
        //    //the context vector
        //    String sContextVector = mtParams != null ? (String)mtParams : "";

        //    StringBuilder postData = new StringBuilder();
        //    for (int i = 0; i < aSourceContents.Length; i++)
        //    {
        //        var sText = aSourceContents[i];
        //        String sSource = GoogleTagsToMTTags(sText);
        //        postData.Append("<tu id=\"" + i + "\">" + HttpUtility.HtmlEncode(sSource) + "</tu>");
        //    }

        //    var aRet = new List<String>();
        //    //give three shot
        //    for (int i = 0; i < 3; i++)
        //    {
        //        try
        //        {
        //            sFrom = ConvertLanguageCode(sFrom);
        //            sTo = ConvertLanguageCode(sTo);

        //            //set the context vector
        //            String sUrl = HOST + "/translate";
        //            //sUrl = String.Format(sUrl, sFrom, sTo, HttpUtility.UrlEncode(sText));
        //            //if (!String.IsNullOrEmpty(sContextVector))
        //            //    sUrl += "&context_vector=" + sContextVector;

        //            WebClient webClient = new WebClient();
        //            webClient.Headers["MMT-ApiKey"] = API_KEY;
        //            webClient.Headers["X-HTTP-Method-Override"] = "GET";
        //            webClient.Headers["MMT-Platform"] = "translatemedia.com";
        //            webClient.Headers["MMT-PluginVersion"] = "1.0";
        //            webClient.Headers["MMT-PlatformVersion"] = "1.0";
        //            webClient.Encoding = Encoding.UTF8;

        //            var reqparm = new NameValueCollection();
        //            reqparm.Add("variant", "true");
        //            reqparm.Add("source", sFrom);
        //            reqparm.Add("target", sTo);
        //            reqparm.Add("q", postData.ToString());
        //            if (!String.IsNullOrEmpty(sContextVector))
        //                reqparm.Add("context_vector", sContextVector);

        //            byte[] responsebytes = webClient.UploadValues(sUrl, "POST", reqparm);
        //            var sResponse = Encoding.UTF8.GetString(responsebytes);


        //            var o = (Newtonsoft.Json.Linq.JToken)JsonConvert.DeserializeObject(sResponse);
        //            var sTranslations = o["data"]["translation"].ToString();

        //            var xmlResult = new XmlDocument();
        //            xmlResult.LoadXml("<xml>" + sTranslations + "</xml>");
        //            var tus = xmlResult.SelectNodes("//tu");
        //            foreach (XmlNode tu in tus)
        //            {
        //                var sTranslation = MTTagsToGoogleTags(tu.InnerXml);
        //                sTranslation = FirstCharToUpper(PostProcessText(sTranslation, sTo, mtParams));
        //                aRet.Add(sTranslation);
        //            }

        //            return aRet.ToArray();
        //        }
        //        catch (WebException ex)
        //        {
        //            var responseStream = (MemoryStream)ex.Response.GetResponseStream();
        //            String responseJson = Encoding.ASCII.GetString(responseStream.ToArray());
        //            cLogManager.DEBUG_LOG("MTErrors.log", "Text:" + aSourceContents[0] + "Error: " + responseJson + "\n");
        //            Thread.Sleep(5000);
        //        }
        //        finally
        //        {
        //        }
        //    }

        //    throw new Exception("MMT ERROR");
        //}

        //public int CreateMemory(String sAccountName, String sSpeciality, MemoryType memoryType)
        //{
        //    try
        //    {
        //        var sTMName = CreateMemoryName(sAccountName, sSpeciality, memoryType);

        //        //get the TM id
        //        if (!TM_LOOKUP.Keys.Contains(sTMName))
        //            UpdateTMLookup();

        //        String sId = "";
        //        if (TM_LOOKUP.Keys.Contains(sTMName))
        //            sId = TM_LOOKUP[sTMName];

        //        //create the TM
        //        if (String.IsNullOrEmpty(sId))
        //        {
        //            using (WebClient webClient = new WebClient())
        //            {
        //                String sUrl = HOST + "/memories";
        //                webClient.Headers["MMT-ApiKey"] = API_KEY;
        //                webClient.Headers["MMT-Platform"] = "translatemedia.com";
        //                webClient.Headers["MMT-PluginVersion"] = "1.0";
        //                webClient.Headers["MMT-PlatformVersion"] = "1.0";
        //                webClient.Encoding = Encoding.UTF8;
        //                var reqparm = new NameValueCollection();
        //                reqparm.Add("name", sTMName);
        //                reqparm.Add("description", sTMName);
        //                byte[] responsebytes = webClient.UploadValues(sUrl, "POST", reqparm);
        //                var response = Encoding.Default.GetString(responsebytes);

        //                var o = (JToken)JsonConvert.DeserializeObject(response);
        //                sId = o["data"]["id"].ToString();
        //                String sName = o["data"]["name"].ToString();
        //                String sKey = o["data"]["key"].ToString();
        //            }
        //        }

        //        return int.Parse(sId);
        //    }
        //    catch (WebException ex)
        //    {
        //        var responseStream = (MemoryStream)ex.Response.GetResponseStream();
        //        String responseJson = Encoding.ASCII.GetString(responseStream.ToArray());
        //        cLogManager.DEBUG_LOG("MTErrors.log", responseJson);

        //        throw new Exception(responseJson);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //    }
        //}

        //public int AddMemoryContent(int idMemory, String sTMXContent)
        //{
        //    int ret = -1;
        //    try
        //    {
        //        String sUrl = HOST + "/memories/" + idMemory.ToString() + "/content";

        //        //upload the file
        //        using (var client = new HttpClient())
        //        {
        //            using (var formData = new MultipartFormDataContent())
        //            {
        //                using (MemoryStream ms = new MemoryStream())
        //                {
        //                    using (GZipStream zipStream = new GZipStream(ms, CompressionMode.Compress, false))
        //                    {
        //                        byte[] contentBytes = Encoding.UTF8.GetBytes(sTMXContent);
        //                        zipStream.Write(contentBytes, 0, contentBytes.Length);
        //                    }
        //                    formData.Headers.Add("MMT-ApiKey", API_KEY);
        //                    formData.Headers.Add("MMT-Platform", "translatemedia.com");
        //                    formData.Headers.Add("MMT-PluginVersion", "1.0");
        //                    formData.Headers.Add("MMT-PlatformVersion", "1.0");
        //                    formData.Add(new ByteArrayContent(ms.ToArray()), "tmx", "data.gz");
        //                    formData.Add(new StringContent("gzip"), "compression");
        //                    var response = client.PostAsync(sUrl, formData).Result;

        //                    var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result);
        //                    var res = sr.ReadToEnd();
        //                    var o = (JToken)JsonConvert.DeserializeObject(res);
        //                    if (!response.IsSuccessStatusCode)
        //                    {
        //                        if (o["status"].ToString() == "500")
        //                        {
        //                            if (o["error"]["type"].ToString() == "EmptyCorpusException")
        //                            {
        //                                cLogManager.DEBUG_LOG("MMTErrors.log", "id memory: " + idMemory + "error: EmptyCorpusException");
        //                                return 0;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            cLogManager.DEBUG_LOG("MMTErrors.log", "id memory: " + idMemory + res);
        //                            cFailureMessages.SendDebugEmail("id memory: " + idMemory + res, "Train MMT error", "alpar.meszaros@toppandigital.com");
        //                            throw new Exception(res); //the response json
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //sId = o["data"]["id"].ToString();
        //                        var size = o["data"]["size"].ToString();
        //                        cLogManager.DEBUG_LOG("MMT.log", "id memory: " + idMemory + " entries: " + size);
        //                        int.TryParse(size, out ret);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        cLogManager.DEBUG_LOG("MTErrors.log", ex.Message);
        //        throw new Exception(ex.Message); // contains the response json
        //    }
        //    finally
        //    {
        //    }

        //    return ret;
        //}

        //public String CreateMemoryName(String sAccountName, String sSpeciality, MemoryType memoryType)
        //{
        //    var oTM = new cTransactionsManager();
        //    String sMemoryName = "__";
        //    if (memoryType == MemoryType.TB)
        //        sMemoryName += "tb_";

        //    sMemoryName += (sAccountName + "_" + sSpeciality).ToLower();
        //    sMemoryName = sMemoryName.Replace('/', '-').Replace(':', '-').Replace('|', '-');

        //    return sMemoryName;
        //}
    }
}
