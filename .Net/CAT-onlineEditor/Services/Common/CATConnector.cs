using CAT.Data;
using CAT.Helpers;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using System.Transactions;
using static ICSharpCode.SharpZipLib.Zip.ZipEntryFactory;
using System.Xml;
using CAT.Enums;
using Microsoft.Extensions.Options;
using CAT.Models.Common;
using Statistics = CAT.Models.Common.Statistics;
using TMMatch = CAT.Models.Common.TMMatch;
using TMAssignment = CAT.Models.Common.TMAssignment;
using AutoMapper;
using System.Security.AccessControl;
using TBEntry = CAT.Models.Common.TBEntry;
using Microsoft.CodeAnalysis.Differencing;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;
using CAT.Models.Entities;
using CAT.Areas.Identity.Data;
using Grpc.Net.Client;
using static Proto.CAT;

namespace CAT.Services.Common
{
    public class CATConnector
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly string _catServerAddress;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        private static int MATCH_THRESHOLD = 50;

        /// <summary>
        /// CATClientService
        /// </summary>
        public CATConnector(DbContextContainer dbContextContainer,
            IConfiguration configuration, IMapper mapper, ILogger<CATConnector> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
            _catServerAddress = _configuration!["CATServer"]!;
        }


        public TMMatch[] GetTMMatches(TMAssignment[] tmAssignments, string sourceXml, string prevXml, string nextXml, string contextID)
        {
            //we can't send over null value
            var aTms = _mapper.Map<Proto.TMAssignment[]>(tmAssignments);

            var grpcChannel = GrpcChannel.ForAddress(_catServerAddress);
            var catClient = new CATClient(grpcChannel);
            var maxHits = 10;
            var request = new Proto.GetTMMatchesRequest
            {
                SourceText = sourceXml,
                PrevText = prevXml,
                NextText = nextXml,
                MatchThreshold = MATCH_THRESHOLD,
                MaxHits = maxHits,
                TMAssignments = { aTms }
            };

            var matches = catClient.GetTMMatches(request);
            var tmMatches = _mapper.Map<TMMatch[]>(matches);

            //convert and remove duplicates
            var finalTMMatches = new Dictionary<String, TMMatch>();
            foreach (var tmMatch in tmMatches)
            {
                String key = tmMatch.source + tmMatch.target;
                if (finalTMMatches.ContainsKey(key))
                    continue;

                tmMatch.source = CATUtils.XmlTags2GoogleTags(tmMatch.source!, CATUtils.TagType.Tmx);
                tmMatch.target = CATUtils.XmlTags2GoogleTags(tmMatch.target!, CATUtils.TagType.Tmx);

                finalTMMatches.Add(key, tmMatch);
            }

            return finalTMMatches.Values.ToArray();
        }

        public TMMatch[] GetConcordance(TMAssignment[] tmAssignments, string searchText, bool caseSensitive, bool searchInTarget)
        {
            //we can't send over null value
            var tmIds = Array.ConvertAll(tmAssignments, tma => tma.tmPath);
            var grpcChannel = GrpcChannel.ForAddress(_catServerAddress);
            var catClient = new CATClient(grpcChannel);
            var request = new Proto.ConcordanceRequest
            {
                SourceText = searchInTarget ? "" : searchText,
                TargetText = searchInTarget ? searchText : "",
                CaseSensitive = caseSensitive,
                TmIds = { tmIds }
            };

            var response = catClient.Concordance(request);

            //convert and remove duplicates
            var finalTMMatches = new Dictionary<string, TMMatch>();
            foreach (var tmEntry in response.TmEntries)
            {
                String key = tmEntry.Source + tmEntry.Target;
                if (finalTMMatches.ContainsKey(key))
                    continue;

                var tmMatch = new TMMatch()
                {
                    id = tmEntry.Id,
                    source = CATUtils.XmlTags2GoogleTags(tmEntry.Source, CATUtils.TagType.Tmx),
                    target = CATUtils.XmlTags2GoogleTags(tmEntry.Target, CATUtils.TagType.Tmx),
                    metadata = tmEntry.Metadata
                };

                finalTMMatches.Add(key, tmMatch);
            }

            return finalTMMatches.Values.ToArray();
        }

        public TBEntry[] ListTBEntries(TBAssignment tBAssignment, String[] languages)
        {
            //var client = GetCATService();
            ////get the TB id from guid
            //var tbEntries = client.ListTBEntries(tBAssignment.idTermbase, languages);

            ////convert the list
            //var lstRet = new List<TBEntry>();
            //foreach (var tbEntry in tbEntries)
            //{
            //    lstRet.Add(new TBEntry()
            //    {
            //        id = tbEntry.id,
            //        terms = tbEntry.terms,
            //        comment = tbEntry.comment,
            //        metadata = tbEntry.metadata
            //    });
            //}

            //return lstRet.ToArray();

            return null;
        }

        public void AddTMEntry(TMAssignment tmAssignment, String sourceXml, String targetXml, String prevXml, String nextXml,
            Dictionary<String, String> metadata)
        {
            try
            {
                //the metadata
                if (metadata == null)
                    metadata = new Dictionary<String, String>();
                if (!String.IsNullOrEmpty(prevXml))
                    metadata.Add("prevSegment", prevXml);
                if (!String.IsNullOrEmpty(nextXml))
                    metadata.Add("nextSegment", nextXml);

                var newEntry = new Proto.TMEntry()
                {
                    Source = sourceXml,
                    Target = targetXml,
                    Metadata = JsonConvert.SerializeObject(metadata)
                };

                var grpcChannel = GrpcChannel.ForAddress(_catServerAddress);
                var catClient = new CATClient(grpcChannel);
                var request = new Proto.AddTMEntriesRequest
                {
                    TMEntries = { newEntry },
                    TmId = tmAssignment.tmPath
                };
                catClient.AddTMEntries(request);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("TM name: " + tmAssignment.tmPath + " AddTMEntry Error: " + ex.ToString());
            }
        }
    }
}
