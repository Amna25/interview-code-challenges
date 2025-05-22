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

        public Fine? ReturnBook(Guid bookStockId){
            using (var context = new LibraryContext())
            {
                var stock = context.Catalogue
                .Include(bs => bs.OnLoanTo)
                .FirstOrDefault(bs => bs.Id == bookStockId);

                if(stock == null){
                    throw new KeyNotFoundException($"No BookStock with Id {bookStockId}");
                };

                if (stock.OnLoanTo == null)
                throw new InvalidOperationException($"BookStock {bookStockId} is not currently on loan");

                Fine? createdFine = null;
                var currentDate = DateTime.UtcNow;
                var dueDate = stock.LoanEndDate!.Value;
                var borrower = stock.OnLoanTo;
                var daysLate = (currentDate.Date - dueDate.Date).Days;
                if(daysLate > 0)
                {
                    var amount = daysLate * 1.0m;
                    var fine   = new Fine
                    {
                        Id = Guid.NewGuid(),
                        BorrowerId = stock.OnLoanTo.Id,
                        BookStockId = stock.Id,
                        Amount = amount,
                        IssuedDate = currentDate,
                        Paid = false
                    };
                    context.Fines.Add(fine);
                    createdFine = fine;
                }

                stock.OnLoanTo = null;
                stock.LoanEndDate = null;
                context.SaveChanges();
                return createdFine;
            }
        }
    }
}