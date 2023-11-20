using CAT.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CAT.Pages
{
    public class TestModel : PageModel
    {
        private readonly IWorkflowService _workflowServie;

        public TestModel(IWorkflowService workflowServie)
        {
            _workflowServie = workflowServie;
        }
        public void OnGet()
        {
            _workflowServie.StartNextStepAsync(1017).GetAwaiter().GetResult(); ;
        }
    }
}
