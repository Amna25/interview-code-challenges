using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface IBorrowerLoansRepository
    {
        public List<BookStock> GetActiveLoans();

        public Fine? ReturnBook(Guid bookStockId);

        public Reservation ReserveTitle(Guid borrowerId, Guid bookId);

        public DateTime GetAvailability(Guid borrowerId, Guid bookId);

        public int GetReservationQueuePosition(Guid borrowerId, Guid bookId);
    }
}