using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class BorrowerLoansRepository : IBorrowerLoansRepository
    {
        public BorrowerLoansRepository()
        {
        }

        public List<BookStock> GetActiveLoans(){
            using (var context = new LibraryContext())
            {
               return context.Catalogue
                .Include(bs => bs.Book)
                .ThenInclude(b => b.Author)
                .Include(bs => bs.OnLoanTo)
                .Where(bs => bs.OnLoanTo != null 
                && bs.LoanEndDate != null 
                && bs.LoanEndDate!.Value >= DateTime.UtcNow).ToList();
            }

        }
    }
}