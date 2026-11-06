using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ICustodianRepository
    {
        PagedResult<Reservation> GetReservationsPaged(string search, long? statusId, int page, int pageSize, long? scopeCustodianUserId = null);

        Reservation GetReservationWithDetails(long reservationId);

        PagedResult<Borrowing> GetActiveBorrowingsPaged(string search, int page, int pageSize, long? scopeCustodianUserId = null);

        int CountActiveBorrowingsForReservation(long reservationId);

        string GetNextTransactionNumber(int year);

        Borrowing GetBorrowingWithDetails(long borrowingId);
    }
}
