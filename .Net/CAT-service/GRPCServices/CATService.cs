using AutoMapper;
using CAT.ConnectedServices.OkapiService;
using Proto;
using CAT.TM;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using CAT.TB;

namespace CAT.GRPCServices
{
    public class CATService : Proto.CAT.CATBase
    {
        private readonly ILogger<CATService> _logger;
        private readonly ITMService _tmService;
        private readonly ITBService _tbService;
        private readonly IMapper _mapper;

        public CATService(ILogger<CATService> logger, ITMService tmService, ITBService tbService, IMapper mapper)
        {
            _logger = logger;
            _tmService = tmService;
            _tbService = tbService;
            _mapper = mapper;
        }

        #region Translation memory
        public override Task<TMExistsResponse> TMExists(TMExistsRequest request, ServerCallContext context)
        {
            try
            {
                var result = _tmService.TMExists(request.TmId);
                return Task.FromResult(new TMExistsResponse { Exists = result });
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<EmptyResponse> CreateTM(CreateTMRequest request, ServerCallContext context)
        {
            try
            {
                _tmService.CreateTM(request.TmId);
                return Task.FromResult(new EmptyResponse());
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }

        }

        public override Task<GetTMInfoResponse> GetTMInfo(GetTMInfoRequest request, ServerCallContext context)
        {
            try
            {
                var tmInfo = _tmService.GetTMInfo(request.TmId, request.FullInfo);

                var response = new GetTMInfoResponse()
                {
                    TmInfo = new Proto.TMInfo()
                    {
                        TmId = tmInfo.tmId,
                        LangFrom = tmInfo.langFrom,
                        LangTo = tmInfo.langTo,
                        LastAccess = Timestamp.FromDateTime(tmInfo.lastAccess.Kind != DateTimeKind.Utc ? tmInfo.lastAccess.ToUniversalTime() : tmInfo.lastAccess),
                        TmType = (TMType)tmInfo.tmType,
                        EntryNumber = tmInfo.entryNumber
                    }
                };

                return Task.FromResult(response);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<GetTMListResponse> GetTMList(GetTMListRequest request, ServerCallContext context)
        {
            try
            {
                var tmList = _tmService.GetTMList(request.FullInfo);
                var response = new GetTMListResponse();
                foreach (var tmInfo in tmList)
                {
                    response.TmInfoList.Add(new Proto.TMInfo
                    {
                        TmId = tmInfo.tmId,
                        LangFrom = tmInfo.langFrom,
                        LangTo = tmInfo.langTo,
                        LastAccess = Timestamp.FromDateTime(tmInfo.lastAccess.Kind != DateTimeKind.Utc ? tmInfo.lastAccess.ToUniversalTime() : tmInfo.lastAccess),
                        TmType = (TMType)tmInfo.tmType,
                        EntryNumber = tmInfo.entryNumber,
                    });
                }
                return Task.FromResult(response);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<GetTMListFromDatabaseResponse> GetTMListFromDatabase(GetTMListFromDatabaseRequest request, ServerCallContext context)
        {
            try
            {
                var tmList = _tmService.GetTMListFromDatabase(request.DbName, request.FullInfo);
                var response = new GetTMListFromDatabaseResponse();
                foreach (var tmInfo in tmList)
                {
                    response.TmInfoList.Add(new Proto.TMInfo
                    {
                        TmId = tmInfo.tmId,
                        LangFrom = tmInfo.langFrom,
                        LangTo = tmInfo.langTo,
                        LastAccess = Timestamp.FromDateTime(tmInfo.lastAccess.Kind != DateTimeKind.Utc ? tmInfo.lastAccess.ToUniversalTime() : tmInfo.lastAccess),
                        TmType = (TMType)tmInfo.tmType,
                        EntryNumber = tmInfo.entryNumber,
                    });
                }

                return Task.FromResult(response);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<GetStatisticsForDocumentResponse> GetStatisticsForDocument(GetStatisticsForDocumentRequest request, ServerCallContext context)
        {
            try
            {
                //var tmAssignments = _mapper.Map<Models.TMAssignment[]>(request.TMAssignments);
                var tmAssignments = request.TMAssignments.Select(tmAssignment => new Models.TMAssignment
                {
                    tmId = tmAssignment.TmId,
                    penalty = tmAssignment.Penalty,
                    speciality = tmAssignment.Speciality,
                }).ToArray();

                var stats = _tmService.GetStatisticsForDocument(request.FileName, request.FileContent.ToByteArray(), request.FilterName,
                    request.FilterContent.ToByteArray(), request.SourceLangISO6391, request.TargetLangsISO6391.ToArray(), tmAssignments);

                var response = new GetStatisticsForDocumentResponse();
                //Array.ForEach(stats, stat => response.Statistics.Add(_mapper.Map<Proto.Statistics>(stat)));
                Array.ForEach(stats, stat => response.Statistics.Add(new Proto.Statistics()
                {
                    SourceLang = stat.sourceLang,
                    TargetLang = stat.targetLang,
                    Repetitions = stat.repetitions,

                    Match101 = stat.match_101,
                    Match100 = stat.match_100,
                    Match9599 = stat.match_95_99,
                    Match8594 = stat.match_85_94,
                    Match7584 = stat.match_75_84,
                    Match5074 = stat.match_50_74,
                    NoMatch = stat.no_match
                }));

                return Task.FromResult(response);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<PreTranslateXliffResponse> PreTranslateXliff(PreTranslateXliffRequest request, ServerCallContext context)
        {
            try
            {
                //var tmAssignments = _mapper.Map<Models.TMAssignment[]>(request.TMAssignments);
                var tmAssignments = request.TMAssignments.Select(tmAssignment => new Models.TMAssignment
                {
                    tmId = tmAssignment.TmId,
                    penalty = tmAssignment.Penalty,
                    speciality = tmAssignment.Speciality,
                }).ToArray();

                var xliffContent = _tmService.PreTranslateXliff(request.XliffContent, request.LangFrom, request.LangTo, tmAssignments, request.MatchThreshold);

                var response = new PreTranslateXliffResponse();
                response.XliffContent = xliffContent;

                return Task.FromResult(response);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<GetTMMatchesResponse> GetTMMatches(GetTMMatchesRequest request, ServerCallContext context)
        {
            try
            {
                //var tmAssignments = _mapper.Map<Models.TMAssignment[]>(request.TMAssignments);
                var tmAssignments = request.TMAssignments.Select(tmAssignment => new Models.TMAssignment
                {
                    tmId = tmAssignment.TmId,
                    penalty = tmAssignment.Penalty,
                    speciality = tmAssignment.Speciality,
                }).ToArray();

                var tmMatches = _tmService.GetTMMatches(tmAssignments, request.SourceText, request.PrevText, request.NextText,
                    (byte)request.MatchThreshold, request.MaxHits);

                var response = new GetTMMatchesResponse();
                Array.ForEach(tmMatches, tmMatch => response.TMMatches.Add(new Proto.TMMatch()
                {
                    Id = tmMatch.id,
                    Source = tmMatch.source,
                    Target = tmMatch.target,
                    Origin = tmMatch.origin,
                    Quality = tmMatch.quality,
                    Metadata = tmMatch.metadata
                }));

                return Task.FromResult(response);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<GetExactMatchResponse> GetExactMatch(GetExactMatchRequest request, ServerCallContext context)
        {
            try
            {
                //var tmAssignments = _mapper.Map<Models.TMAssignment[]>(request.TMAssignments);
                var tmAssignments = request.TMAssignments.Select(tmAssignment => new Models.TMAssignment
                {
                    tmId = tmAssignment.TmId,
                    penalty = tmAssignment.Penalty,
                    speciality = tmAssignment.Speciality,
                }).ToArray();

                var tmMatch = _tmService.GetExactMatch(tmAssignments, request.SourceText, request.PrevText, request.NextText);

                var response = new GetExactMatchResponse();
                if (tmMatch != null)
                {
                    response.TMMatch = new Proto.TMMatch()
                    {
                        Id = tmMatch.id,
                        Source = tmMatch.source,
                        Target = tmMatch.target,
                        Origin = tmMatch.origin,
                        Quality = tmMatch.quality,
                        Metadata = tmMatch.metadata
                    };
                }

                return Task.FromResult(response);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<AddTMEntriesResponse> AddTMEntries(AddTMEntriesRequest request, ServerCallContext context)
        {
            try
            {
                var tmEntries = request.TMEntries.Select(tmEntry => new Models.TMEntry
                {
                    source = tmEntry.Source,
                    target = tmEntry.Target,
                    metadata = tmEntry.Metadata,

                }).ToArray();
                var entriesNum = _tmService.AddTMEntries(request.TmId, tmEntries);

                var response = new AddTMEntriesResponse() { EntriesNum = entriesNum };

                return Task.FromResult(response);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<EmptyResponse> DeleteTMEntry(DeleteTMEntryRequest request, ServerCallContext context)
        {
            try
            {
                _tmService.DeleteTMEntry(request.TmId, request.EntryId);

                return Task.FromResult(new EmptyResponse());
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }

        }

        public override Task<ConcordanceResponse> Concordance(ConcordanceRequest request, ServerCallContext context)
        {
            try
            {
                var tmEntries = _tmService.Concordance(request.TmIds.ToArray(), request.SourceText, request.TargetText, request.CaseSensitive, request.MaxHits);

                var response = new ConcordanceResponse();
                Array.ForEach(tmEntries, tmEntry => response.TmEntries.Add(new Proto.TMEntry()
                {
                    Id = tmEntry.id,
                    Source = tmEntry.source,
                    Target = tmEntry.target,
                    Metadata = tmEntry.metadata,
                }));

                return Task.FromResult(response);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<ExportTmxResponse> ExportTmx(ExportTmxRequest request, ServerCallContext context)
        {
            try
            {
                var tmxContent = _tmService.ExportTmx(request.TmId);
                var response = new ExportTmxResponse() { TmxContent = tmxContent };

                return Task.FromResult(response);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<ImportTmxResponse> ImportTmx(ImportTmxRequest request, ServerCallContext context)
        {
            try
            {
                var importResult = _tmService.ImportTmx(request.TmId, request.SourceLangIso6391, request.TargetLangIso6391,
                    request.TmxContent, request.User, request.Speciality);
                var response = new ImportTmxResponse();
                response.TmxImportResult.AllItems = importResult.allItems;
                response.TmxImportResult.ImportedItems = importResult.importedItems;

                return Task.FromResult(response);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }
        #endregion Translation memory

        #region Termbase
        public override Task<CreateTBResponse> CreateTB(CreateTBRequest request, ServerCallContext context)
        {
            try
            {
                var tbInfo = _tbService.CreateTB((Enums.TBType)request.TbType, request.IdType, request.LangCodes.ToArray());
                var result = new CreateTBResponse()
                {
                    TbInfo = new TBInfo()
                    {
                        Id = tbInfo.id,
                        Metadata = tbInfo.metadata
                    }
                };
                result.TbInfo.Languages.AddRange(tbInfo.languages);

                return Task.FromResult(result);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<GetTBInfoResponse> GetTBInfo(GetTBInfoRequest request, ServerCallContext context)
        {
            try
            {
                var tbInfo = _tbService.GetTBInfo((Enums.TBType)request.TbType, request.IdType);
                var result = new GetTBInfoResponse()
                {
                    TbInfo = new TBInfo()
                    {
                        Id = tbInfo.id,
                        Metadata = tbInfo.metadata
                    }
                };
                result.TbInfo.Languages.AddRange(tbInfo.languages);

                return Task.FromResult(result);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<GetTBInfoByIdResponse> GetTBInfoById(GetTBInfoByIdRequest request, ServerCallContext context)
        {
            try
            {
                var tbInfo = _tbService.GetTBInfo(request.TermbaseId);
                var result = new GetTBInfoByIdResponse()
                {
                    TbInfo = new TBInfo()
                    {
                        Id = tbInfo.id,
                        Metadata = tbInfo.metadata
                    }
                };
                result.TbInfo.Languages.AddRange(tbInfo.languages);

                return Task.FromResult(result);
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<EmptyResponse> AddLanguageToTB(AddLanguageToTBRequest request, ServerCallContext context)
        {
            try
            {
                _tbService.AddLanguageToTB(request.TermbaseId, request.LangCode);

                return Task.FromResult(new EmptyResponse());
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<EmptyResponse> RemoveLanguageFromTB(RemoveLanguageFromTBRequest request, ServerCallContext context)
        {
            try
            {
                _tbService.RemoveLanguageFromTB(request.TermbaseId, request.LangCode);

                return Task.FromResult(new EmptyResponse());
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<AddOrUpdateTBEntryResponse> AddOrUpdateTBEntry(AddOrUpdateTBEntryRequest request, ServerCallContext context)
        {
            try
            {
                var tbEntry = new Models.TBEntry()
                {
                    id = request.TbEntry.Id,
                    terms = request.TbEntry.Terms.ToDictionary(entry => entry.Key, entry => entry.Value),
                    metadata = request.TbEntry.Metadata,
                };
                _tbService.AddOrUpdateTBEntry(request.TermbaseId, tbEntry, request.User);

                return Task.FromResult(new AddOrUpdateTBEntryResponse());
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<EmptyResponse> DeleteTBEntry(DeleteTBEntryRequest request, ServerCallContext context)
        {
            try
            {
                _tbService.DeleteTBEntry(request.TermbaseId, request.EntryId);

                return Task.FromResult(new EmptyResponse());
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<ImportTBResponse> ImportTB(ImportTBRequest request, ServerCallContext context)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<ImportTBEntriesResponse> ImportTBEntries(ImportTBEntriesRequest request, ServerCallContext context)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }

        public override Task<ListTBEntriesResponse> ListTBEntries(ListTBEntriesRequest request, ServerCallContext context)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex) // Catching general exception
            {
                // Log the exception
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."), ex.Message);
            }
        }
        #endregion Termbase
    }
}