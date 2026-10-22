using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ViewModelClasses;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Borrower portal dashboard (screen 2.0).
    /// </summary>
    [Authorize(Roles = RoleCodes.Borrower)]
    public class BorrowerController : ControllerBase<BorrowerController>
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentUserService _currentUser;

        public BorrowerController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IDashboardService dashboardService,
            ICurrentUserService currentUser)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._dashboardService = dashboardService;
            this._currentUser = currentUser;
        }

        public IActionResult Index() => this.RedirectToAction(nameof(this.Dashboard));

        public IActionResult Dashboard()
        {
            var userId = this._currentUser.GetActorUserId();
            if (!userId.HasValue)
            {
                return this.RedirectToAction("Login", "Account");
            }

            try
            {
                return this.View(this._dashboardService.GetBorrowerDashboard(userId.Value));
            }
            catch (Exception ex)
            {
                this.HandleExceptionLog(ex, "Dashboard");
                this.TempData["ErrorMessage"] = "Unable to load your dashboard.";
                return this.View(new BorrowerDashboardViewModel { DisplayName = "Borrower" });
            }
        }
    }
}
