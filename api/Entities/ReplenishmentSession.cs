namespace api.Entities;

public class ReplenishmentSession
{
    public int Id { get; set; }
    public ICollection<DateBatch> DateBatches { get; set; } = [];
}
