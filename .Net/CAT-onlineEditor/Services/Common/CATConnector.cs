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
using Proto;
using Microsoft.CodeAnalysis.Host;
using TMType = CAT.Enums.TMType;

namespace CAT.Services.Common
{
    public class CatConnector
    {
        private readonly ILanguageService _languageService;
        private readonly ICatClientFactory _catClientFactory;
        private readonly ILogger _logger;

        private static readonly int MATCH_THRESHOLD = 50;

        /// <summary>
        /// CATClientService
        /// </summary>
        public CatConnector(ILanguageService languageService,
            ICatClientFactory catClientFactory, ILogger<CatConnector> logger)
        {
            _languageService = languageService;
            _catClientFactory = catClientFactory;
            _logger = logger;
        }

        private CATClient GetCatClient()
        {
            return _catClientFactory.CreateClient();
        }


        public TMMatch[] GetTMMatches(TMAssignment[] tmAssignments, string sourceXml, string prevXml, string nextXml)
        {
            var tms = Array.ConvertAll(tmAssignments, tma => new Proto.TMAssignment()
            {
                TmId = tma.tmId,
                Penalty = tma.penalty,
                Speciality = tma.speciality
            });

            var catClient = GetCatClient();
            var maxHits = 10;
            var request = new Proto.GetTMMatchesRequest
            {
                SourceText = sourceXml,
                PrevText = prevXml ?? "",
                NextText = nextXml ?? "",
                MatchThreshold = MATCH_THRESHOLD,
                MaxHits = maxHits,
                TMAssignments = { tms }
            };

            var response = catClient.GetTMMatches(request);
            var tmMatches = Array.ConvertAll(response.TMMatches.ToArray(), match => new TMMatch()
            {
                id = int.Parse(match.Id),
                source = match.Source,
                target = match.Target,
                origin = match.Origin,
                quality = match.Quality,
                metadata = match.Metadata
            });

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
            var tmIds = Array.ConvertAll(tmAssignments, tma => tma.tmId);
            var catClient = GetCatClient();
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
            throw new NotFiniteNumberException();
        }

        public void AddTMEntry(TMAssignment tmAssignment, String sourceXml, String targetXml, String prevXml, String nextXml,
            Dictionary<String, String> metadata)
        {
            try
            {
                //the metadata
                metadata ??= new Dictionary<String, String>();
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

                var catClient = GetCatClient();
                var request = new Proto.AddTMEntriesRequest
                {
                    TMEntries = { newEntry },
                    TmId = tmAssignment.tmId
                };
                catClient.AddTMEntries(request);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("TM name: {tmId} AddTMEntry Error: {ex}", tmAssignment.tmId, ex.ToString());
            }
        }

        public TMAssignment[] GetTMAssignments(int companyId, int sourceLang, int targetLang, int speciality, bool createTM)
        {
            var tmAssignments = new List<TMAssignment>();
            //only company TM
            var catClient = GetCatClient();

            var tmId = CreateTMId(companyId, companyId, sourceLang, targetLang, TMType.CompanyPrimary);
            var tmExistsRequest = new TMExistsRequest { TmId = tmId };
            var exists = catClient.TMExists(tmExistsRequest).Exists;
            if (!exists && createTM)
            {
                var createTMRequest = new CreateTMRequest() { TmId = tmId };
                catClient.CreateTM(createTMRequest);
                exists = true;
            }

            if (exists)
            {
                var tmAssignment = new TMAssignment()
                {
                    isGlobal = false,
                    isReadonly = false,
                    penalty = 0,
                    speciality = speciality,
                    tmId = tmId
                };
                tmAssignments.Add(tmAssignment);
            }

            return tmAssignments.ToArray();
        }

        private String CreateTMId(int groupId, int companyId, int sourceLang, int targetLang, TMType type)
        {
            var sourceLangIso639_1 = _languageService.GetLanguageCodeIso639_1(sourceLang).Result;
            var targetLangIso639_1 = _languageService.GetLanguageCodeIso639_1(targetLang).Result;
            var tmPrefix = "";
            if (type == TMType.Global)
                tmPrefix = "$_";
            else if (type == TMType.GroupPrimary)
                tmPrefix = "_";
            else if (type == TMType.GroupSecondary)
                tmPrefix = "_sec_";
            else if (type == TMType.CompanyPrimary)
                tmPrefix = "__";
            else if (type == TMType.CompanySecondary)
                tmPrefix = "__sec_";

            return groupId + "/" + tmPrefix + companyId + "_" + sourceLangIso639_1 + "_" + targetLangIso639_1;
        }
    }
}
