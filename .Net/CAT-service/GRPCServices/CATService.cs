using AutoMapper;
using CAT.ConnectedServices.OkapiService;
using Proto;
using CAT.TM;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace CAT.GRPServices
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
            var tmInfo = _tmService.GetTMInfo(request.Id, request.FullInfo);

            var response = new GetTMInfoResponse()
            {
                Id = tmInfo.id,
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
                    Id = tmInfo.id,
                    LangFrom = tmInfo.langFrom,
                    LangTo = tmInfo.langTo,
                    LastAccess = Timestamp.FromDateTime(tmInfo.lastAccess.Kind != DateTimeKind.Utc ? tmInfo.lastAccess.ToUniversalTime() : tmInfo.lastAccess),
                    TmType = (TMType)tmInfo.tmType,
                    EntryNumber = tmInfo.entryNumber,
                });
            }
            return Task.FromResult(response);
        }
    }
}