using AutoMapper;
using CAT.ConnectedServices.OkapiService;
using Proto;
using CAT.TM;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace CAT.GRPCServices
{
    public class CATService : Proto.CAT.CATBase
    {
        private readonly ILogger<CATService> _logger;
        private readonly ITMService _tmService;
        private readonly IMapper _mapper;

        public CATService(ILogger<CATService> logger, ITMService tmService, IMapper mapper)
        {
            _logger = logger;
            _tmService = tmService;
            _mapper = mapper;
        }

        public override Task<TMExistsResponse> TMExists(TMExistsRequest request, ServerCallContext context)
        {
            var result = _tmService.TMExists(request.TmId);
            return Task.FromResult(new TMExistsResponse { Exists = result });
        }

        public override Task<EmptyResponse> CreateTM(CreateTMRequest request, ServerCallContext context)
        {
            _tmService.CreateTM(request.TmId);
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<GetTMInfoResponse> GetTMInfo(GetTMInfoRequest request, ServerCallContext context)
        {
            var tmInfo = _tmService.GetTMInfo(request.TmId, request.FullInfo);

            var response = new GetTMInfoResponse()
            {
                TmId = tmInfo.tmId,
                LangFrom = tmInfo.langFrom,
                LangTo = tmInfo.langTo,
                LastAccess = Timestamp.FromDateTime(tmInfo.lastAccess.Kind != DateTimeKind.Utc ? tmInfo.lastAccess.ToUniversalTime() : tmInfo.lastAccess),
                TmType = (TMType)tmInfo.tmType,
                EntryNumber = tmInfo.entryNumber,
            };

            return Task.FromResult(response);
        }

        public override Task<GetTMListResponse> GetTMList(GetTMListRequest request, ServerCallContext context)
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

        public override Task<GetTMListFromDatabaseResponse> GetTMListFromDatabase(GetTMListFromDatabaseRequest request, ServerCallContext context)
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

        public override Task<GetStatisticsForDocumentResponse> GetStatisticsForDocument(GetStatisticsForDocumentRequest request, ServerCallContext context)
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

        public override Task<PreTranslateXliffResponse> PreTranslateXliff(PreTranslateXliffRequest request, ServerCallContext context)
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

        public override Task<GetTMMatchesResponse> GetTMMatches(GetTMMatchesRequest request, ServerCallContext context)
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

        public override Task<GetExactMatchResponse> GetExactMatch(GetExactMatchRequest request, ServerCallContext context)
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
    }
}