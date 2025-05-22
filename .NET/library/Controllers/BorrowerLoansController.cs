using Microsoft.AspNetCore.Mvc;
using OneBeyondApi.DataAccess;
using System.Collections;
using OneBeyondApi.ModelDtos;

namespace OneBeyondApi.Controllers 
{
    [ApiController]
    [Route("[controller]")]
    public class BorrowerLoansController : ControllerBase
    {
        private readonly ILogger<BorrowerLoansController> _logger;
        private readonly IBorrowerLoansRepository _borrowerLoansRepository;

        public BorrowerLoansController(ILogger<BorrowerLoansController> logger, IBorrowerLoansRepository borrowerLoansRepository)
        {
            _logger = logger;
            _borrowerLoansRepository = borrowerLoansRepository;   
        }

        [HttpGet("GetActiveLoans")]
        public ActionResult<List<BorrowerLoansDto>> GetActiveLoans()
        {
        var activeStocks = _borrowerLoansRepository.GetActiveLoans();
        var result = activeStocks
            .GroupBy(bs => bs.OnLoanTo!.Id)
            .Select(g => new BorrowerLoansDto {
                BorrowerId   = g.Key,
                BorrowerName = g.First().OnLoanTo!.Name,
                EmailAddress = g.First().OnLoanTo!.EmailAddress,
                LoanEndDate  = g.Min(bs => bs.LoanEndDate!.Value),
                BookLoans    = g.Select(bs => new BookLoanItemDto {
                                BookStockId = bs.Id,
                                Title = bs.Book.Name}).ToList()
            })
            .ToList();

            return Ok(result);
        }

        [HttpPost("Return/{bookStockId:guid}")]
        public ActionResult<FineDto?> ReturnBook(Guid bookStockId)
        {
            try
            {
                var fine = _borrowerLoansRepository.ReturnBook(bookStockId);
                if(fine == null){
                    return NoContent();
                }
                var dueFine = new FineDto{
                   FineId = fine.Id, 
                   BorrowerId = fine.BorrowerId, 
                   BookStockId = fine.BookStockId, 
                   Amount = fine.Amount, 
                   IssuedDate = fine.IssuedDate, 
                   Paid = fine.Paid};
                return Ok(dueFine);
           
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"BookStock {bookStockId} not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }

        }

    }

    
}