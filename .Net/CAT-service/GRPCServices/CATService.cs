using CAT.ConnectedServices.OkapiService;
using CAT.TM;
using Grpc.Core;

namespace CAT.GRPServices
{
    public class CATService : CAT.CATBase
    {
        private readonly ILogger<CATService> _logger;
        private readonly ITMService _tmService;

        public CATService(ILogger<CATService> logger, ITMService tmService)
        {
            _logger = logger;
            _tmService = tmService;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        //public override Task<TMExistsResponse> TMExists(TMExistsRequest request, ServerCallContext context)
        //{
        //    var result = _tmService.TMExists(request.TmId);
        //    return Task.FromResult(new TMExistsResponse { Exists = result });
        //}

        //public override Task<EmptyResponse> CreateTM(CreateTMRequest request, ServerCallContext context)
        //{
        //    _tmService.CreateTM(request.TmId);
        //    return Task.FromResult(new EmptyResponse());
        //}

        //public override Task<TMInfoResponse> GetTMInfo(GetTMInfoRequest request, ServerCallContext context)
        //{
        //    var tmInfo = _tmService.GetTMInfo(request.Id, request.FullInfo);
        //    return Task.FromResult(new TMInfoResponse { Id = tmInfo.Id, Info = tmInfo.Info }); // Simplified; adjust as needed
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