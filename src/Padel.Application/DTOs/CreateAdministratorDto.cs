using System.ComponentModel.DataAnnotations;
using Padel.Domain.Entities;

namespace Padel.Application.DTOs;

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
    public AdministratorType Type { get; set; }

    public int? SiteId { get; set; }
}
