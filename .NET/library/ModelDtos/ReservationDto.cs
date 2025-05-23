namespace OneBeyondApi.ModelDtos
{
    public class ReservationDto
    {
        public Guid ReservationId { get; set; }
        public int Position { get; set; }
        public DateTime AvailableAt {get; set;}
    }
}