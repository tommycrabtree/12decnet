namespace api.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UnitsPerCase { get; set; }
    public int FreezerUnits { get; set; }
    public bool Active { get; set; } = true;
    public int ShelfCapacity { get; set; }
    public int ShelfDaysAllowed { get; set; }
}
