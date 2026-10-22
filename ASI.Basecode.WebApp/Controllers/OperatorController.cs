using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using ASI.Basecode.Resources.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Stand-in identity: lets a developer switch the operator/borrower acting on the data
    /// while web auth is stripped. Sets the CurrentUserId/CurrentUserName session keys read
    /// by CurrentUserService.
    /// </summary>
    [Authorize(Roles = RoleCodes.Admin)]
    public class OperatorController : ControllerBase<OperatorController>
    {
        private const string CurrentUserIdKey = "CurrentUserId";
        private const string CurrentUserNameKey = "CurrentUserName";

        private readonly IUserService _userService;

        public OperatorController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUserService userService)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._userService = userService;
        }

        /// <summary>
        /// Shows the current acting operator and the available user accounts.
        /// </summary>
        public IActionResult Index(string returnUrl)
        {
            try
            {
                var users = this._userService.GetOperatorOptions();

                var currentId = this._session.GetString(CurrentUserIdKey);
                this.ViewBag.UserOptions = new SelectList(
                    users.Select(u => new { Id = u.UserId.ToString(), Text = $"{u.LastName}, {u.FirstName} ({u.AccountIdentifier})" }),
                    "Id",
                    "Text",
                    currentId);
                this.ViewBag.CurrentName = this._session.GetString(CurrentUserNameKey);
                this.ViewBag.ReturnUrl = returnUrl;

                return this.View();
            }
            catch (Exception ex)
            {
                this.HandleExceptionLog(ex, "Index");
                this.TempData["ErrorMessage"] = "Unable to load the operator list.";
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Switches the acting operator for this session.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Select(long userId, string returnUrl)
        {
            var user = this._userService.GetByIdWithDetails(userId);
            if (user == null)
            {
                this.TempData["ErrorMessage"] = ASI.Basecode.Resources.Messages.Errors.UserNotFound;
                return this.RedirectToAction(nameof(this.Index));
            }

            var displayName = $"{user.FirstName} {user.LastName}";
            this._session.SetString(CurrentUserIdKey, user.UserId.ToString());
            this._session.SetString(CurrentUserNameKey, displayName);

            this.TempData["SuccessMessage"] = $"Now acting as {displayName} ({user.AccountIdentifier}).";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction("Index", "Users");
        }
    }
}
