namespace api.DTOs;

public class ReplenishmentSessionDto
{
    public int Id { get; set; }
    public ICollection<DateBatchDto> DateBatches { get; set; } = [];
}
