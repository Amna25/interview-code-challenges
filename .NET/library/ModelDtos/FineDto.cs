namespace OneBeyondApi.ModelDtos
{
    public class FineDto
    {
        public Guid FineId { get; set; }
        public Guid BorrowerId { get; set; }
        public Guid  BookStockId{ get; set; }
        public decimal Amount { get; set; }
        public DateTime IssuedDate{ get; set; }
        public bool Paid { get; set; }
    }
}
