using System.ComponentModel.DataAnnotations;

namespace Padel.Frontend.Models;

public class CreateMemberDto
{
    [Required(ErrorMessage = "Le prénom est requis.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est requis.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis.")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le type de membre est requis.")]
    public string MemberType { get; set; } = "Global";

    public int? SiteId { get; set; }
}
