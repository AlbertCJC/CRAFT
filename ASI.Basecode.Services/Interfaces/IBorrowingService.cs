using ASI.Basecode.Services.ViewModelClasses;

namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Borrower borrowings: screens 2.6 My Borrowings, 2.7 Borrowing Details and 2.8 Borrowing History.
    /// </summary>
    public interface IBorrowingService
    {
        BorrowingMineViewModel GetMine(long userId, string search);

        BorrowingDetailsViewModel GetDetails(long userId, long borrowingId);

        BorrowingHistoryViewModel GetHistory(long userId, string search, string condition);
    }
}
