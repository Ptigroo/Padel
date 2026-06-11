using System.ComponentModel.DataAnnotations;

namespace Padel.Frontend.Models;

public class CreateAdministratorDto
{
    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Type { get; set; }

    public int? SiteId { get; set; }
}
