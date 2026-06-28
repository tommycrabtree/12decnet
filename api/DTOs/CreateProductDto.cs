using System.ComponentModel.DataAnnotations;

namespace api.DTOs;

public class CreateProductDto
{
    [Required]
    public string DisplayName { get; set; } = "";

    [Required]
    public int ShelfCapacity { get; set; }

    [Required]
    public int ShelfDaysAllowed { get; set; }
}
