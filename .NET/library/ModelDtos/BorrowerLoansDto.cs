using System;
using System.Collections.Generic;
namespace OneBeyondApi.ModelDtos
{
    public class BorrowerLoansDto
    {
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public DateTime LoanEndDate { get; set; }
        public IList<BookLoanItemDto> BookLoans { get; set; } = new List<BookLoanItemDto>();

    }
}