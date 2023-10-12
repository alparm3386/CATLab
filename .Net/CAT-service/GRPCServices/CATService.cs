using AutoMapper;
using CAT.ConnectedServices.OkapiService;
using CAT.TM;
using Grpc.Core;

namespace CAT.GRPServices
{
    public class CATService : CAT.CATBase
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
            //var tmInfo = _tmService.GetTMInfo(request.Id, request.FullInfo);
            //var response = _mapper.Map<TMInfoResponse>(tmInfo);
            return Task.FromResult(new GetTMInfoResponse());
        }

        //public override Task<TMInfoResponse> GetTMInfo(TMInfoRequest request, ServerCallContext context)
        //{
        //    //var tmInfo = _tmService.GetTMInfo(request.Id, request.FullInfo);
        //    //var response = _mapper.Map<TMInfoResponse>(tmInfo);
        //    var response = new TMInfoResponse();
        //    return Task.FromResult(response);
        //}

        //public override Task<TMListResponse> GetTMList(GetTMListRequest request, ServerCallContext context)
        //{
        //    var tmList = _tmService.GetTMList(request.FullInfo);
        //    var response = new TMListResponse();
        //    foreach (var tmInfo in tmList)
        //    {
        //        response.TMInfos.Add(new TMInfo { Id = tmInfo.Id, Info = tmInfo.Info }); // Simplified; adjust as needed
        //    }
        //    return Task.FromResult(response);
        //}
    }
}