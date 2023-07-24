using CAT_web.Data;
using CAT_web.Helpers;
using CAT_web.Models;
using CATService;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Transactions;

namespace CAT_web.Services.CAT
{
    public class CATClientService
    {
        private readonly CAT_webContext _context;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// CATClientService
        /// </summary>
        public CATClientService(CAT_webContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private EndpointAddress GetCATServiceEndpoint()
        {
            var endPointAddr = "net.tcp://10.0.20.55:8086";

            //local test
            //endPointAddr = "net.tcp://localhost:8086";
            //create the endpoint address
            return new EndpointAddress(endPointAddr);
        }

        /// <summary>
        /// GetCATServiceBinding
        /// </summary>
        /// <returns></returns>
        private static NetTcpBinding GetCATServiceBinding(int timeoutInMinutes)
        {
            TimeSpan timeSpan;
            if (timeoutInMinutes > 0)
                timeSpan = new TimeSpan(0, timeoutInMinutes, 0); // new TimeSpan(timeout * 10000);
            else
                timeSpan = new TimeSpan(0, 5, 0); // 5 minutes

            //set up the binding for the TCP connection
            NetTcpBinding tcpBinding = new NetTcpBinding();

            tcpBinding.Name = "NetTcpBinding_ICATService";
            tcpBinding.CloseTimeout = timeSpan;
            tcpBinding.OpenTimeout = timeSpan;
            tcpBinding.ReceiveTimeout = timeSpan;
            tcpBinding.SendTimeout = timeSpan;
            tcpBinding.TransferMode = TransferMode.Buffered;
            tcpBinding.MaxBufferPoolSize = 524288 * 1000;
            tcpBinding.MaxBufferSize = 65536 * 10000;
            tcpBinding.ReaderQuotas.MaxDepth = 32;
            tcpBinding.ReaderQuotas.MaxStringContentLength = 8192 * 100000;
            tcpBinding.ReaderQuotas.MaxArrayLength = 16384 * 10000;
            tcpBinding.ReaderQuotas.MaxBytesPerRead = 4096000;
            tcpBinding.ReaderQuotas.MaxNameTableCharCount = 1638400000;
            tcpBinding.ReliableSession.Ordered = true;
            tcpBinding.ReliableSession.InactivityTimeout = new TimeSpan(0, 10, 0);
            tcpBinding.ReliableSession.Enabled = false;
            tcpBinding.Security.Mode = SecurityMode.None;

            return tcpBinding;
        }

        public ICATService GetCATService()
        {
            ChannelFactory<ICATService> channelFactory =
                new ChannelFactory<ICATService>(GetCATServiceBinding(5), GetCATServiceEndpoint());

            foreach (OperationDescription op in channelFactory.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractBehavior != null)
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
            }

            return channelFactory.CreateChannel();
        }

        public bool CanBeParsed(String sFilePath)
        {
            //this supposed to be on connector level
            String sExt = Path.GetExtension(sFilePath).ToLower();

            if (sExt == ".doc" || sExt == ".docx" || sExt == ".xls" || sExt == ".xlsx" || sExt == ".ppt" || sExt == ".pptx"
               || sExt == ".htm" || sExt == ".html" || sExt == ".txt" || sExt == ".rtf" || sExt == ".xml" || sExt == ".xlf"
               || sExt == ".xliff" || sExt == ".mqxliff" || sExt == ".sdlxliff" || sExt == ".mqxlz" || sExt == ".xlz" /*|| sExt == ".pdf"*/ || sExt == ".mdd"
               || sExt == ".resx" || sExt == ".strings" || sExt == ".csv" || sExt == ".wsxz"
               || sExt == ".json" || sExt == ".idml" || sExt == ".sdlppx"
               || (ConfigurationSettings.AppSettings["PdfSupport"] == "true" && sExt == ".pdf"))
                return true;

            return false;
        }

        private String GetDefaultFilter(String sFilePath)
        {
            var sExt = Path.GetExtension(sFilePath).ToLower();
            var sFilterName = "";
            switch (sExt)
            {
                case ".docx":
                case ".xlsx":
                case ".pptx":
                    sFilterName = "okf_openxml@tm-okf_openxml.fprm";
                    break;
                default:
                    sFilterName = "";
                    break;
            }

            if (String.IsNullOrEmpty(sFilterName))
                return null;

            var fileFiltersFolder = Path.Combine(_configuration["FileFiltersFolder"]);
            var sFilterPath = Path.Combine(fileFiltersFolder, sFilterName);

            return sFilterPath;
        }

        public Statistics[] GetStatisticsForDocument(string sFilePath, string sFilterPath, String sourceLang,
            string[] aTargetLangs)
        {
            List<String> lstFilesToDelete = new List<String>();
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                if (!CanBeParsed(sFilePath))
                    throw new Exception("File type cannot be parsed");

                //filter
                if (String.IsNullOrEmpty(sFilterPath))
                    sFilterPath = GetDefaultFilter(sFilePath);

                var client = GetCATService();

                //var aTMSettings = GetTMSettings(1044, idFromLng, aIdTargetLangs, sSpeciality, false);

                if (CATUtils.IsCompressedMemoQXliff(sFilePath))
                {
                    sFilePath = CATUtils.ExtractMQXlz(sFilePath);
                    lstFilesToDelete.Add(sFilePath);
                }

                //set the parameters
                String sFilename = Path.GetFileName(sFilePath);
                byte[] fileContent = File.ReadAllBytes(sFilePath);
                String sFiltername = Path.GetFileName(sFilterPath);
                byte[] filterContent = null;
                if (File.Exists(sFilterPath))
                    filterContent = File.ReadAllBytes(sFilterPath);
                TMAssignment[] aTMs = null;

                //the target language array
                var lstTargetLangs = new List<String>();
                var aRet = new List<Statistics>();
                foreach (string sTargetLang in aTargetLangs)
                {
                    var stats = client.GetStatisticsForDocument(sFilename, fileContent, sFiltername, filterContent,
                    sourceLang, new string[] { sTargetLang }, aTMs);
                    aRet.Add(new Statistics()
                    {
                        repetitions = stats[0].repetitions,
                        match_101 = stats[0].match_101,
                        match_100 = stats[0].match_100,
                        match_95_99 = stats[0].match_95_99,
                        match_85_94 = stats[0].match_85_94,
                        match_75_84 = stats[0].match_75_84,
                        match_50_74 = stats[0].match_50_74,
                        no_match = stats[0].no_match,
                        targetLang = sTargetLang
                    });

                }

                sw.Stop();
                return aRet.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //clean-up
                foreach (var tmpFileName in lstFilesToDelete)
                    try
                    {
                        File.Delete(tmpFileName);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
            }
        }
    }
}
