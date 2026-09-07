namespace api.DTOs;

public class DateBatchDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ReplenishmentSessionId { get; set; }
    public int RequestUnits { get; set; }
    public int DateUnits { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public int? FiftyPercentOffUnits { get; set; }
    public int? DonationUnits { get; set; }
}
