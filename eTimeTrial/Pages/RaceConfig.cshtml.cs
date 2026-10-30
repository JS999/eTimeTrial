using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eTimeTrial.Web.Pages
{
    
    public class RaceConfig : PageModel
    {
        private readonly ILogger<RaceConfig> _logger;

        public RaceConfig(ILogger<RaceConfig> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }

}
