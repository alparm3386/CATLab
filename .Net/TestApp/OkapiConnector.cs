using log4net.Core;
using Okapi;
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

namespace TestApp
{
    /// <summary>
    /// OkapiConnector
    /// </summary>
    public class OkapiConnector
    {
        private BasicHttpBinding _binding;

        /// <summary>
        /// OkapiConnector
        /// </summary>
        /// <param name="iServer"></param>
        public OkapiConnector()
        {
            _binding = GetOkapiServiceBinding();
        }

        private EndpointAddress GetOkapiServiceEndpoint()
        {
            //var endPointAddr = "http://159.223.246.57:8080/services/OkapiService";
            var endPointAddr = "http://localhost:8080/services/OkapiService";

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
        private OkapiService GetOkapiService()
        {
            ChannelFactory<OkapiService> channelFactory =
                new ChannelFactory<OkapiService>(_binding, GetOkapiServiceEndpoint());

            foreach (OperationDescription op in channelFactory.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractBehavior != null)
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
            }

            return channelFactory.CreateChannel();
        }

        public string CreateXliffFromDocument(string fileName, byte[] fileContent, string filterName, byte[] filterContent, string sourceLang,
                    string targetLang)
        {
            try
            {
                //the client
                var okapiClient = GetOkapiService();
                string sXliffContent = okapiClient.createXliffAsync(new createXliffRequest(fileName, fileContent, filterName, filterContent, sourceLang,
                    targetLang, null)).Result.createXliffReturn;

                return sXliffContent;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public byte[] CreateDocumentFromXliff(string fileName, byte[] fileContent, string filterName, byte[] filterContent,
            string sourceLangISO639_1, string targetLangISO639_1, string xliffContent)
        {
            try
            {
                //the client
                var okapiClient = GetOkapiService();
                var bytes = okapiClient.createDocumentFromXliffAsync(new createDocumentFromXliffRequest(fileName, fileContent, filterName, filterContent, sourceLangISO639_1,
                    targetLangISO639_1, xliffContent)).Result.createDocumentFromXliffReturn;

                return bytes;
            }
            catch (Exception ex)
            {
                //re-try on the failover server
                throw;
            }
        }

    }
}
