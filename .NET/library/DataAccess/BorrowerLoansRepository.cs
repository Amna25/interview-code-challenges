using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;
using System.Linq;   

namespace OneBeyondApi.DataAccess
{
    public class BorrowerLoansRepository : IBorrowerLoansRepository
    {

        public BorrowerLoansRepository()
        {
        }

        public List<BookStock> GetActiveLoans(){
            using var context = new LibraryContext();
            return context.Catalogue
                .Include(bs => bs.Book)
                .ThenInclude(b => b.Author)
                .Include(bs => bs.OnLoanTo)
                .Where(bs => bs.OnLoanTo != null 
                && bs.LoanEndDate != null 
                && bs.LoanEndDate!.Value >= DateTime.UtcNow).ToList();
        }

        public Fine? ReturnBook(Guid bookStockId){
            using var context = new LibraryContext();
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

        public Reservation ReserveTitle(Guid borrowerId, Guid bookId)
        {
            using var context = new LibraryContext();
            
                var borrower = context.Borrowers.Find(borrowerId)
                  ?? throw new KeyNotFoundException($"Borrower {borrowerId} not found");
                var book = context.Books.Find(bookId)
                  ?? throw new KeyNotFoundException($"Book {bookId} not found");

                var reservBook = new Reservation {
                    Id = Guid.NewGuid(),
                    BorrowerId = borrowerId,
                    BookId = bookId,
                    ReservedAt = DateTime.UtcNow
                };
                context.Reservations.Add(reservBook);
                context.SaveChanges();
                return reservBook;
            
        }

        public DateTime GetAvailability(Guid borrowerId, Guid bookId)
        {
            using var context = new LibraryContext();
            
           var queue = BuildReservationQueue(bookId, context);

            var position = queue.FindIndex(id => id == borrowerId);
            if (position < 0)
                throw new KeyNotFoundException("No reservation found for this borrower/title");

            var allCopies = context.Catalogue.Where(bs => bs.Book.Id == bookId).ToList();
            var activeLoans = allCopies
                .Where(bs => bs.OnLoanTo != null && bs.LoanEndDate >= DateTime.UtcNow)
                .OrderBy(bs => bs.LoanEndDate)
                .ToList();

            var freeNow = allCopies.Count - activeLoans.Count;
            if (position < freeNow)
                return DateTime.UtcNow;

            var index = position - freeNow;
            if (index < activeLoans.Count)
                return activeLoans[index].LoanEndDate!.Value;

            return activeLoans.Last().LoanEndDate!.Value; 
        }

        public int GetReservationQueuePosition(Guid borrowerId, Guid bookId)
        {
            using var context = new LibraryContext();
            var queue = BuildReservationQueue(bookId, context);

            var zeroBasedPos = queue.FindIndex(id => id == borrowerId);
            if (zeroBasedPos < 0)
                throw new KeyNotFoundException($"No reservation found for Borrower {borrowerId} on Book {bookId}");

            return zeroBasedPos + 1;
        }

        private List<Guid> BuildReservationQueue(Guid bookId, LibraryContext ctx) =>
        ctx.Reservations
            .Where(r => r.BookId == bookId)
            .OrderBy(r => r.ReservedAt)
            .Select(r => r.BorrowerId)
            .ToList();
    }
}