using System.ComponentModel.DataAnnotations;

namespace api.DTOs;

public class RegisterDto
{
    [Required]
    public required string DisplayName { get; set; } = "";

    [Required]
    [EmailAddress]
    public required string Email { get; set; } = "";

    [Required]
    [MinLength(4)]
    public required string Password { get; set; } = "";
}
