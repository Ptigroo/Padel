using System.ComponentModel.DataAnnotations;

namespace Padel.Frontend.Models;

public class CreateCourtDto
{
    [Required(ErrorMessage = "Le nom est requis.")]
    public string Name { get; set; } = string.Empty;

    public int SiteId { get; set; }
}
