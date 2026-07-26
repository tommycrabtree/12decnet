namespace api.DTOs;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
    public int UnitsPerCase { get; set; }
    public int FreezerUnits { get; set; }
    public int ShelfCapacity { get; set; }
    public int ShelfDaysAllowed { get; set; }
}
