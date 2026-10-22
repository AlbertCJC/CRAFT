using ASI.Basecode.Data;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ViewModelClasses;
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
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// User Management screens (Figma: A - User Management / A - User Details).
    /// Thin controller: binds requests, delegates to IUserService, prepares view data.
    /// </summary>
    [Authorize(Roles = RoleCodes.Admin)]
    public class UsersController : ControllerBase<UsersController>
    {
        private readonly IUserService _userService;

        public UsersController(
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
        /// User list with search, codebook filters and paging (10/25 per page).
        /// </summary>
        public IActionResult Index(
            string search,
            long? role,
            long? department,
            long? status,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var model = this._userService.GetIndexViewModel(
                    search, role, department, status, page, pageSize);

                this.ViewBag.RoleOptions = this.ToSelectList(
                    this._userService.GetRoleOptions(), model.RoleId, "All Roles");
                this.ViewBag.DepartmentOptions = this.ToSelectList(
                    this._userService.GetDepartmentOptions(), model.DepartmentId, "All Departments");
                this.ViewBag.StatusOptions = this.ToSelectList(
                    this._userService.GetAccountStatusOptions(), model.AccountStatusId, "All Statuses");
                this.ViewBag.PageSizeOptions = new SelectList(
                    new[] { new { Id = "10", Text = "10 per page" }, new { Id = "25", Text = "25 per page" }, },
                    "Id", "Text", model.PageSize.ToString());

                return this.View(model);
            }
            catch (Exception ex)
            {
                this.HandleExceptionLog(ex, "Index");
                this.TempData["ErrorMessage"] = "Unable to load the user list.";
                return this.View(new UserIndexViewModel { Page = new PagedResult<UserListViewModel>() });
            }
        }

        public IActionResult Create()
        {
            this.ViewBag.RoleOptions = this.ToSelectList(this._userService.GetRoleOptions(), null, "-- Select role --");
            this.ViewBag.DepartmentOptions = this.ToSelectList(this._userService.GetDepartmentOptions(), null, "-- None --");
            this.ViewBag.StatusOptions = this.ToSelectList(this._userService.GetAccountStatusOptions(), null, "-- Select status --");

            return this.View(new UserFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserFormViewModel form)
        {
            if (!this.ModelState.IsValid)
            {
                this.PopulateFormSelectLists(form.RoleId, form.DepartmentId, form.AccountStatusId);
                return this.View(form);
            }

            var (success, error, _) = this._userService.Save(form);
            if (!success)
            {
                this.ModelState.AddModelError(string.Empty, error);
                this.PopulateFormSelectLists(form.RoleId, form.DepartmentId, form.AccountStatusId);
                return this.View(form);
            }

            this.TempData["SuccessMessage"] = "User account created.";
            return this.RedirectToAction(nameof(this.Index));
        }

        public IActionResult Edit(long id)
        {
            var form = this._userService.GetForEdit(id);
            if (form == null)
            {
                this.TempData["ErrorMessage"] = ASI.Basecode.Resources.Messages.Errors.UserNotFound;
                return this.RedirectToAction(nameof(this.Index));
            }

            this.PopulateFormSelectLists(form.RoleId, form.DepartmentId, form.AccountStatusId);
            return this.View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(long id, UserFormViewModel form)
        {
            form.UserId = id;

            if (!this.ModelState.IsValid)
            {
                this.PopulateFormSelectLists(form.RoleId, form.DepartmentId, form.AccountStatusId);
                return this.View(form);
            }

            var (success, error, _) = this._userService.Save(form);
            if (!success)
            {
                this.ModelState.AddModelError(string.Empty, error);
                this.PopulateFormSelectLists(form.RoleId, form.DepartmentId, form.AccountStatusId);
                return this.View(form);
            }

            this.TempData["SuccessMessage"] = "User account updated.";
            return this.RedirectToAction(nameof(this.Index));
        }

        /// <summary>
        /// Read-only user profile detail view (Figma: A - User Details).
        /// </summary>
        public IActionResult Details(long id)
        {
            var entity = this._userService.GetByIdWithDetails(id);
            if (entity == null)
            {
                this.TempData["ErrorMessage"] = ASI.Basecode.Resources.Messages.Errors.UserNotFound;
                return this.RedirectToAction(nameof(this.Index));
            }

            return this.View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            var (success, error) = this._userService.Delete(id);
            if (success)
            {
                this.TempData["SuccessMessage"] = "User account deleted.";
            }
            else
            {
                this.TempData["ErrorMessage"] = error;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        private void PopulateFormSelectLists(long? roleId, long? departmentId, long? accountStatusId)
        {
            this.ViewBag.RoleOptions = this.ToSelectList(this._userService.GetRoleOptions(), roleId, "-- Select role --");
            this.ViewBag.DepartmentOptions = this.ToSelectList(this._userService.GetDepartmentOptions(), departmentId, "-- None --");
            this.ViewBag.StatusOptions = this.ToSelectList(this._userService.GetAccountStatusOptions(), accountStatusId, "-- Select status --");
        }

        private SelectList ToSelectList(IEnumerable<LookupViewModel> options, long? selected, string emptyText)
        {
            var items = options
                .Select(o => new { Id = o.Id.ToString(), o.Text })
                .ToList();

            // emptyText row first
            var withEmpty = new[] { new { Id = string.Empty, Text = emptyText } }.Concat(items);

            return new SelectList(withEmpty, "Id", "Text", selected?.ToString());
        }
    }
}
