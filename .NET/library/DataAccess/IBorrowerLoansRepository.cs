using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public interface IBorrowerLoansRepository
    {
        public List<BookStock> GetActiveLoans();

        public void ReturnBook(Guid bookStockId);
    }
}