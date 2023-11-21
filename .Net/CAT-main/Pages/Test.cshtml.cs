using CAT.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CAT.Pages
{
    public class TestModel : PageModel
    {
        private readonly IWorkflowService _workflowServie;
        private readonly IOrderService _orderService;

        public TestModel(IWorkflowService workflowServie, IOrderService orderService)
        {
            _workflowServie = workflowServie;
            _orderService = orderService;
        }
        public void OnGet()
        {
            _orderService.FinalizeOrderAsync(1018).GetAwaiter().GetResult();
            //_workflowServie.StartWorkflowAsync(1017).GetAwaiter().GetResult();
        }
    }
}
