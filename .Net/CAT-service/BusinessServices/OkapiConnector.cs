using cat.utils;
using OkapiService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;
using System.Xml;

namespace CAT.BusinessServices
{
    /// <summary>
    /// OkapiConnector
    /// </summary>
    public class OkapiConnector : IOkapiConnector
    {
        private BasicHttpBinding _binding;
        private ILogger _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// OkapiConnector
        /// </summary>
        /// <param name="iServer"></param>
        public OkapiConnector(IConfiguration configuration, ILogger<OkapiConnector> logger)
        {
            _binding = GetOkapiServiceBinding();
            _configuration = configuration;
            _logger = logger;
        }

        private EndpointAddress GetOkapiServiceEndpoint()
        {
            var endPointAddr = "http://" + _configuration["OkapiServer"] + ":8080/OkapiService/services/OkapiService";

            //create the endpoint address for the 
            return new EndpointAddress(endPointAddr);
        }

        /// <summary>
        /// GetServiceBinding
        /// </summary>
        /// <returns></returns>
        private BasicHttpBinding GetOkapiServiceBinding()
        {
            //set up the binding for the TCP connection
            BasicHttpBinding httpBinding = new BasicHttpBinding();

            httpBinding.Name = "BasicHttpBinding_OkapiService";
            httpBinding.CloseTimeout = new TimeSpan(0, 30, 0);
            httpBinding.OpenTimeout = new TimeSpan(0, 30, 0);
            httpBinding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            httpBinding.SendTimeout = new TimeSpan(0, 30, 0);
            httpBinding.MaxBufferPoolSize = 524288 * 100;
            httpBinding.MaxBufferSize = 16384 * 100000;
            httpBinding.MaxReceivedMessageSize = 16384 * 100000;
            httpBinding.ReaderQuotas.MaxDepth = 32;
            httpBinding.ReaderQuotas.MaxStringContentLength = 8192 * 100000;
            httpBinding.ReaderQuotas.MaxArrayLength = 16384 * 1000;
            httpBinding.ReaderQuotas.MaxBytesPerRead = 4096 * 100;
            httpBinding.ReaderQuotas.MaxNameTableCharCount = 16384 * 10000;

            return httpBinding;
        }

        /// <summary>
        /// GetOkapiService
        /// </summary>
        /// <returns></returns>
        private IOkapiService GetOkapiService()
        {
            ChannelFactory<IOkapiService> channelFactory =
                new ChannelFactory<IOkapiService>(_binding, GetOkapiServiceEndpoint());

            foreach (OperationDescription op in channelFactory.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractBehavior != null)
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
            }

            return channelFactory.CreateChannel();
        }

        public String CreateXliffFromDocument(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent, String sourceLang,
                    String targetLang)
        {
            try
            {
                //the client
                var okapiClient = GetOkapiService();
                String sXliffContent = okapiClient.createXliffAsync(new createXliffRequest(sFileName, fileContent, sFilterName, filterContent, sourceLang,
                    targetLang, null)).Result.createXliffReturn;

                return sXliffContent;
            }
            catch (Exception ex)
            {
                _logger.LogError("Okapi service -> ERROR: CreateXliffFromDocument -> endpoint default " + ex.ToString());
                throw;
            }
        }

        public byte[] CreateDocumentFromXliff(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent,
            String sourceLangISO639_1, String targetLangISO639_1, String sXliffContent)
        {
            try
            {
                //the client
                var okapiClient = GetOkapiService();
                var bytes = okapiClient.createDocumentFromXliffAsync(new createDocumentFromXliffRequest(sFileName, fileContent, sFilterName, filterContent, sourceLangISO639_1,
                    targetLangISO639_1, sXliffContent)).Result.createDocumentFromXliffReturn;

                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError("Okapi service.log -> ERROR: CreateDocumentFromXliff -> endpoint default " + ex.ToString());
                //re-try on the failover server
                throw;
            }
        }

    }
}
