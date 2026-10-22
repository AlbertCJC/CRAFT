using ASI.Basecode.Resources.Constants;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Home Controller — the public landing page and the role router.
    /// </summary>
    public class HomeController : ControllerBase<HomeController>
    {
        public HomeController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
        }

        /// <summary>
        /// [1.0 Landing] — public when signed out; otherwise routes to the role dashboard.
        /// </summary>
        public IActionResult Index()
        {
            if (this.User?.Identity?.IsAuthenticated != true)
            {
                return this.View();
            }

            return this.RedirectToRoleHome(this.User.FindFirst(ClaimTypes.Role)?.Value);
        }

        private IActionResult RedirectToRoleHome(string roleCode)
        {
            switch (roleCode)
            {
                case RoleCodes.Admin:
                    return this.RedirectToAction("Dashboard", "Monitoring");
                case RoleCodes.Technician:
                    return this.RedirectToAction("Dashboard", "Custodian");
                case RoleCodes.Student:
                case RoleCodes.Faculty:
                    return this.RedirectToAction("Dashboard", "Borrower");
                default:
                    return this.View("Index");
            }
        }
    }
}
