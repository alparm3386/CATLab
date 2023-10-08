using cat.utils;
using CATService.OkapiService;
using Newtonsoft.Json;
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

namespace okapi
{
    /// <summary>
    /// OkapiConnector
    /// </summary>
    public class OkapiConnector
    {
        private BasicHttpBinding _binding;
        private static Logger logger = new Logger();

        /// <summary>
        /// OkapiConnector
        /// </summary>
        /// <param name="iServer"></param>
        public OkapiConnector()
        {
            _binding = GetOkapiServiceBinding();
        }

        private EndpointAddress GetOkapiServiceEndpoint(bool bFailover)
        {
            var endPointAddr = "";

            if (!bFailover)
                endPointAddr = "http://" + ConfigurationSettings.AppSettings["OkapiServer"] + ":8080/OkapiService/services/OkapiService";
            else
                endPointAddr = "http://" + ConfigurationSettings.AppSettings["OkapiFailoverServer"] + ":8080/OkapiService/services/OkapiService";

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
            //httpBinding.TransactionFlow = false;
            //httpBinding.TransferMode = TransferMode.Buffered;
            //httpBinding.TransactionProtocol = TransactionProtocol.OleTransactions;
            httpBinding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
            //httpBinding.ListenBacklog = 200;
            httpBinding.MaxBufferPoolSize = 524288 * 100;
            httpBinding.MaxBufferSize = 16384 * 100000;
            httpBinding.MaxReceivedMessageSize = 16384 * 100000;
            //httpBinding.MaxConnections = 100;
            httpBinding.ReaderQuotas.MaxDepth = 32;
            httpBinding.ReaderQuotas.MaxStringContentLength = 8192 * 100000;
            httpBinding.ReaderQuotas.MaxArrayLength = 16384 * 1000;
            httpBinding.ReaderQuotas.MaxBytesPerRead = 4096 * 100;
            httpBinding.ReaderQuotas.MaxNameTableCharCount = 16384 * 10000;
            //httpBinding.ReliableSession.Ordered = true;
            //httpBinding.ReliableSession.InactivityTimeout = new TimeSpan(0, 10, 0);
            //httpBinding.ReliableSession.Enabled = false;
            //httpBinding.Security.Mode = BasicHttpSecurityMode.None;

            //tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            //tcpBinding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            //tcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;

            return httpBinding;
        }

        /// <summary>
        /// GetOkapiService
        /// </summary>
        /// <returns></returns>
        public IOkapiService GetOkapiService()
        {
            ChannelFactory<IOkapiService> channelFactory =
                new ChannelFactory<IOkapiService>(_binding, GetOkapiServiceEndpoint(false));

            foreach (OperationDescription op in channelFactory.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractBehavior != null)
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
            }

            return channelFactory.CreateChannel();
        }

        /// <summary>
        /// GetOkapiService
        /// </summary>
        /// <param name="bFailover"></param>
        /// <returns></returns>
        public IOkapiService GetOkapiService(bool bFailover)
        {
            ChannelFactory<IOkapiService> channelFactory =
                new ChannelFactory<IOkapiService>(_binding, GetOkapiServiceEndpoint(bFailover));

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
                String sXliffContent = okapiClient.createXliff(new createXliffRequest(sFileName, fileContent, sFilterName, filterContent, sourceLang,
                    targetLang, null)).createXliffReturn;

                return sXliffContent;
            }
            catch (Exception ex)
            {
                logger.Log("Okapi service.log", "ERROR: CreateXliffFromDocument -> endpoint default " + ex.ToString());
                //re-try on the failover server
                try
                {
                    var okapiClient = GetOkapiService(true);
                    String sXliffContent = okapiClient.createXliff(new createXliffRequest(sFileName, fileContent, sFilterName, filterContent, sourceLang,
                        targetLang, null)).createXliffReturn;
                    return sXliffContent;
                }
                catch (Exception exFailover)
                {
                    logger.Log("Okapi service.log", "ERROR: CreateXliffFromDocument -> endpoint failover " + exFailover.ToString());
                    throw ex;
                }
            }
        }

        public byte[] CreateDocumentFromXliff(String sFileName, byte[] fileContent, String sFilterName, byte[] filterContent,
            String sourceLangISO639_1, String targetLangISO639_1, String sXliffContent)
        {
            try
            {
                //the client
                var okapiClient = GetOkapiService();
                var bytes = okapiClient.createDocumentFromXliff(new createDocumentFromXliffRequest(sFileName, fileContent, sFilterName, filterContent, sourceLangISO639_1,
                    targetLangISO639_1, sXliffContent)).createDocumentFromXliffReturn;

                return bytes;
            }
            catch (Exception ex)
            {
                logger.Log("Okapi service.log", "ERROR: CreateDocumentFromXliff -> endpoint default " + ex.ToString());
                //re-try on the failover server
                try
                {
                    var okapiClient = GetOkapiService(true);
                    var bytes = okapiClient.createDocumentFromXliff(new createDocumentFromXliffRequest(sFileName, fileContent, sFilterName, filterContent, sourceLangISO639_1,
                        targetLangISO639_1, sXliffContent)).createDocumentFromXliffReturn;

                    return bytes;
                }
                catch (Exception exFailover)
                {
                    logger.Log("Okapi service.log", "ERROR: CreateDocumentFromXliff -> endpoint failover " + exFailover.ToString());
                    throw ex;
                }
            }
        }

    }
}
