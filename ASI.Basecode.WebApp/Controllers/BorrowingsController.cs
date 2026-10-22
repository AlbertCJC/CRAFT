using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Borrower borrowings: 2.6 My Borrowings, 2.7 Borrowing Details, 2.8 Borrowing History.
    /// </summary>
    [Authorize(Roles = RoleCodes.Borrower)]
    public class BorrowingsController : ControllerBase<BorrowingsController>
    {
        private readonly IBorrowingService _borrowingService;
        private readonly ICurrentUserService _currentUser;

        public BorrowingsController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IBorrowingService borrowingService,
            ICurrentUserService currentUser)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._borrowingService = borrowingService;
            this._currentUser = currentUser;
        }

        public IActionResult Mine(string search)
        {
            var userId = this._currentUser.GetActorUserId();
            if (!userId.HasValue)
            {
                return this.RedirectToAction("Login", "Account");
            }

            return this.View(this._borrowingService.GetMine(userId.Value, search));
        }

        public IActionResult Details(long id)
        {
            var userId = this._currentUser.GetActorUserId();
            if (!userId.HasValue)
            {
                return this.RedirectToAction("Login", "Account");
            }

            var model = this._borrowingService.GetDetails(userId.Value, id);
            if (model == null)
            {
                this.TempData["ErrorMessage"] = "Borrowing record not found.";
                return this.RedirectToAction(nameof(this.Mine));
            }

            return this.View(model);
        }

        public IActionResult History(string search, string condition)
        {
            var userId = this._currentUser.GetActorUserId();
            if (!userId.HasValue)
            {
                return this.RedirectToAction("Login", "Account");
            }

            return this.View(this._borrowingService.GetHistory(userId.Value, search, condition));
        }
    }
}
