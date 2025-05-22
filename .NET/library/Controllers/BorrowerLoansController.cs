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
                BookTitles   = g.Select(bs => bs.Book.Name).ToList()
            })
            .ToList();

        return Ok(result);
    }

    }

    
}