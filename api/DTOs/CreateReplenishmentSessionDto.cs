namespace api.DTOs;

public class CreateReplenishmentSessionDto
{
    public ICollection<CreateDateBatchDto> DateBatches { get; set; } = [];
}
