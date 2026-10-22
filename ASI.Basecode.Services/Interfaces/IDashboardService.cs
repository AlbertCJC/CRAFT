using ASI.Basecode.Services.ViewModelClasses;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Read models for the role-specific dashboards (2.0 borrower, 3.0 custodian).
    /// </summary>
    public interface IDashboardService
    {
        BorrowerDashboardViewModel GetBorrowerDashboard(long userId);

        CustodianDashboardViewModel GetCustodianDashboard();
    }
}
